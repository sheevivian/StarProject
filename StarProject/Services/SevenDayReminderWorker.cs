using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StarProject.Models;

namespace StarProject.Services
{
	public class SevenDayReminderWorker : BackgroundService
	{
		private readonly IServiceProvider _sp;
		private readonly ILogger<SevenDayReminderWorker> _logger;

		// Windows: "Taipei Standard Time"；若部署於 Linux 請改 "Asia/Taipei"
		private readonly TimeZoneInfo _tz = TimeZoneInfo.FindSystemTimeZoneById("Taipei Standard Time");

		public SevenDayReminderWorker(IServiceProvider sp, ILogger<SevenDayReminderWorker> logger)
		{
			_sp = sp;
			_logger = logger;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				var nowLocal = TimeZoneInfo.ConvertTime(DateTime.UtcNow, _tz);

				// === 排程 ===
				// 驗證用：每 5 秒執行一次；上線請改為每天 09:00（台北）
				var nextRunLocal = nowLocal.AddSeconds(5);
				// var nextRunLocal = new DateTime(nowLocal.Year, nowLocal.Month, nowLocal.Day, 9, 0, 0);
				// if (nowLocal >= nextRunLocal) nextRunLocal = nextRunLocal.AddDays(1);

				var delay = nextRunLocal - nowLocal;
				await Task.Delay(delay, stoppingToken);

				try
				{
					using var scope = _sp.CreateScope();
					var db = scope.ServiceProvider.GetRequiredService<StarProjectContext>();
					var mail = scope.ServiceProvider.GetRequiredService<MailService>();

					// === 視窗：今天 00:00（含）～ 第 7 天 24:00（不含）→ 含第七天整日 ===
					var startLocal = nowLocal.Date;         // e.g. 2025/09/13 00:00
					var endLocal = startLocal.AddDays(8); // e.g. 2025/09/21 00:00（含 9/20 一整天）
					var startUtc = TimeZoneInfo.ConvertTimeToUtc(startLocal, _tz);
					var endUtc = TimeZoneInfo.ConvertTimeToUtc(endLocal, _tz);

					// A 本地時間比較（StartDate 存本地時間時會命中）
					var eventsLocal = await db.Events
						.Where(e => e.StartDate >= startLocal && e.StartDate < endLocal)
						.ToListAsync(stoppingToken);

					// B UTC 比較（StartDate 存 UTC 時會命中）
					var eventsUtc = await db.Events
						.Where(e => e.StartDate >= startUtc && e.StartDate < endUtc)
						.ToListAsync(stoppingToken);

					// 合併去重（以 Event.No 為鍵）
					var events = eventsLocal.Concat(eventsUtc)
											.GroupBy(e => e.No)
											.Select(g => g.First())
											.ToList();

					_logger.LogInformation("[Reminder7d] Tick {NowLocal}. WindowLocal {Start}~{End}. Events(Local={Lc}, Utc={Uc}, Distinct={All})",
						nowLocal, startLocal, endLocal, eventsLocal.Count, eventsUtc.Count, events.Count);

					foreach (var ev in events)
					{
						var people = await db.Participants
							.Where(p => p.EventNo == ev.No &&
									   (p.Status == "Success" || p.Status == "報名成功"))
							.Include(p => p.UsersNoNavigation)
							.ToListAsync(stoppingToken);

						int sent = 0, skipped = 0, failed = 0;

						foreach (var p in people)
						{
							var to = p.UsersNoNavigation?.Email;

							// ★★ 關鍵：一律先落 Pending（即使沒有 Email）
							var notif = new EventNotif
							{
								EventNo = ev.No,
								ParticipantNo = p.No,
								Category = "Reminder7d",
								Status = "Pending",
								Senttime = DateTime.UtcNow
							};

							try
							{
								db.EventNotifs.Add(notif);
								await db.SaveChangesAsync(stoppingToken); // 若曾寄過，這裡會丟 DbUpdateException
							}
							catch (DbUpdateException)
							{
								// 已有相同 (EventNo, ParticipantNo, Category) → 視為已處理，略過
								skipped++;
								continue;
							}

							// 沒有 Email → 立刻標記失敗（但台帳已留）
							if (string.IsNullOrWhiteSpace(to))
							{
								try { notif.Status = "Failed"; await db.SaveChangesAsync(stoppingToken); } catch { }
								failed++;
								continue;
							}

							try
							{
								await mail.SendEventReminderEmail(to, ev.Title, ev.StartDate);

								notif.Status = "Success";
								// 如需把 Senttime 視為實際寄出時間可改為：notif.Senttime = DateTime.UtcNow;
								await db.SaveChangesAsync(stoppingToken);
								sent++;
							}
							catch (Exception ex)
							{
								try { notif.Status = "Failed"; await db.SaveChangesAsync(stoppingToken); } catch { }
								failed++;
								_logger.LogError(ex, "[Reminder7d] Send failed. P#{Pid}, E#{Eid}", p.No, ev.No);
							}
						}

						_logger.LogInformation("[Reminder7d] Event#{Eid} \"{Title}\" Start={Start} → sent={Sent}, skipped={Skip}, failed={Fail}",
							ev.No, ev.Title, ev.StartDate, sent, skipped, failed);
					}
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "SevenDayReminderWorker run failed.");
				}
			}
		}
	}
}
