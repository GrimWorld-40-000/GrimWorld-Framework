using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;

namespace ColorPicker
{
	public class TextField<T>
	{
		private T _value;
		private readonly string _id;
		private string _temp;
		private readonly Func<string, bool> _validator;
		private readonly Func<string, T> _parser;
		private readonly Func<T, string> _toString;
		private readonly Action<T> _callback;
		private bool _spinner;

		public TextField(T value, string id, Action<T> callback, Func<string, T> parser = null,
			Func<string, bool> validator = null, Func<T, string> toString = null, bool spinner = false)
		{
			_value = value;
			_id = id;
			_temp = value.ToString();
			_callback = callback;
			_validator = validator;
			_parser = parser;
			_toString = toString;
			_spinner = spinner;
		}

		public T Value
		{
			get => _value;
			set
			{
				_value = value;
				var toString = _toString;
				_temp = (toString != null ? toString(value) : null) ?? value.ToString();
			}
		}

		public static TextField<float> Float01(float value, string id, Action<float> callback)
		{
			Func<string, float> parser = float.Parse;
			Func<string, bool> validator = Validate01;
			return new TextField<float>(value, id, callback, parser, validator,
				f => Round(f).ToString(), true);
		}

		public static TextField<string> Hex(string value, string id, Action<string> callback)
		{
			Func<string, string> parser = hex => hex;
			Func<string, bool> validator = ValidateHex;
			return new TextField<string>(value, id, callback, parser, validator);
		}

		public void Draw(Rect rect)
		{
			var validator = _validator;
			GUI.color = validator == null || validator(_temp) ? Color.white : Color.red;
			GUI.SetNextControlName(_id);
			var text = Widgets.TextField(rect, _temp);
			GUI.color = Color.white;
			var flag = text != _temp;
			if (flag)
			{
				_temp = text;
				var validator2 = _validator;
				var flag2 = validator2 == null || validator2(_temp);
				if (flag2)
				{
					_value = _parser(_temp);
					var callback = _callback;
					if (callback != null) callback(_value);
				}
			}
		}

		private static bool Validate01(string value)
		{
			var flag = !float.TryParse(value, out var num);
			return !flag && num >= 0f && num <= 1f;
		}

		private static bool ValidateHex(string value)
		{
			return ColorUtility.TryParseHtmlString(value, out _);
		}

		private static float Round(float value, int digits = 2)
		{
			var num = Mathf.Pow(10f, digits);
			return Mathf.RoundToInt(value * num) / num;
		}
	}
}
