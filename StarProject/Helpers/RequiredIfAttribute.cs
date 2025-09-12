using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class RequiredIfAttribute : ValidationAttribute
{
	private readonly string _dependentProperty;
	private readonly object _targetValue;

	public RequiredIfAttribute(string dependentProperty, object targetValue)
	{
		_dependentProperty = dependentProperty;
		_targetValue = targetValue;
	}

	protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
	{
		// 取得依賴屬性
		PropertyInfo? property = validationContext.ObjectType.GetProperty(_dependentProperty);
		if (property == null)
			return new ValidationResult($"未知屬性 {_dependentProperty}");

		object? dependentValue = property.GetValue(validationContext.ObjectInstance);

		// 當依賴屬性等於目標值時，檢查當前欄位是否有值
		if ((dependentValue?.ToString() ?? "") == _targetValue.ToString())
		{
			if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
			{
				return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} 為必填");
			}
		}

		return ValidationResult.Success;
	}
}

