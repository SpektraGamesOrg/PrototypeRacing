using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

namespace Analytics
{
    [System.Serializable]
    public struct AnalyticsEventParameter
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum ParameterType
        {
            LONG = 0, // INT
            DOUBLE = 1, // FLOAT
            STRING = 2
        }

        public ParameterType parameterType;
        public long longValue;
        public double doubleValue;
        public string stringValue;

        public string ConvertToString()
        {
            if (parameterType == ParameterType.LONG)
            {
                return longValue.ToString();
            }
            else if (parameterType == ParameterType.DOUBLE)
            {
                return doubleValue.ToString();
            }
            else
            {
                return stringValue;
            }
        }

        public AnalyticsEventParameter(object value)
        {
            if (ReferenceEquals(value, null))
            {
                parameterType = ParameterType.STRING;
                longValue = 0;
                doubleValue = 0;
                stringValue = null;
                return;
            }

            Type type = value.GetType();

            if (type == typeof(string))
            {
                parameterType = ParameterType.STRING;
                longValue = 0;
                doubleValue = 0;
                stringValue = value as string;
            }
            else if (type == typeof(float))
            {
                parameterType = ParameterType.DOUBLE;
                longValue = 0;
                doubleValue = (float)value;
                stringValue = null;
            }
            else if (type == typeof(int))
            {
                parameterType = ParameterType.DOUBLE;
                longValue = (int)value;
                doubleValue = 0;
                stringValue = null;
            }
            else if (type == typeof(double))
            {
                parameterType = ParameterType.DOUBLE;
                longValue = 0;
                doubleValue = (double)value;
                stringValue = null;
            }
            else if (type == typeof(long))
            {
                parameterType = ParameterType.DOUBLE;
                longValue = (long)value;
                doubleValue = 0;
                stringValue = null;
            }
            else
            {
                Debug.LogError("Undefined Type: " + type.ToString());
                parameterType = ParameterType.STRING;
                longValue = 0;
                doubleValue = 0;
                stringValue = null;
            }
        }

        public static AnalyticsEventParameter LongParam(long value) // INT
        {
            return new AnalyticsEventParameter()
            {
                parameterType = ParameterType.LONG,
                longValue = value,
                doubleValue = 0,
                stringValue = null
            };
        }

        public static AnalyticsEventParameter IntParam(int value) // INT
        {
            return LongParam((long)value);
        }

        public static AnalyticsEventParameter DoubleParam(double value) // FLOAT
        {
            return new AnalyticsEventParameter()
            {
                parameterType = ParameterType.DOUBLE,
                longValue = 0,
                doubleValue = value,
                stringValue = null
            };
        }

        public static AnalyticsEventParameter FloatParam(float value) // FLOAT
        {
            return DoubleParam((double)value);
        }

        public static AnalyticsEventParameter StringParam(string value)
        {
            return new AnalyticsEventParameter()
            {
                parameterType = ParameterType.STRING,
                longValue = 0,
                doubleValue = 0,
                stringValue = value
            };
        }
    }
}