using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace _Game.Scripts.Editor.JsonEditor
{
    /// <summary>
    /// Editor-only classification of a JSON string, produced by <see cref="JsonValidation.Validate"/>.
    /// </summary>
    internal enum JsonStatus
    {
        /// <summary>Null / whitespace-only. Not an error - a fallback can legitimately be empty.</summary>
        Empty,

        /// <summary>Parses cleanly with no issues.</summary>
        Valid,

        /// <summary>Parses, but with a non-blocking issue worth flagging (e.g. duplicate object keys).</summary>
        Warning,

        /// <summary>Does not parse. <see cref="JsonValidationResult.Message"/> carries the reason.</summary>
        Invalid,
    }

    /// <summary>Immutable result of validating a JSON string.</summary>
    internal readonly struct JsonValidationResult
    {
        public readonly JsonStatus Status;
        public readonly string Message;
        public readonly int Line;
        public readonly int Column;

        /// <summary>The parsed token (non-null only when <see cref="Status"/> is Valid or Warning).</summary>
        public readonly JToken Token;

        private JsonValidationResult(JsonStatus status, string message, int line, int column, JToken token)
        {
            Status = status;
            Message = message;
            Line = line;
            Column = column;
            Token = token;
        }

        /// <summary>Empty and Valid/Warning are safe to write back; Invalid is not.</summary>
        public bool CanApply => Status != JsonStatus.Invalid;

        /// <summary>True when a parsed <see cref="Token"/> is available (Valid or Warning).</summary>
        public bool HasToken => Token != null;

        public static JsonValidationResult Empty() =>
            new JsonValidationResult(JsonStatus.Empty, "Empty - no JSON.", 0, 0, null);

        public static JsonValidationResult Valid(JToken token, int nodeCount) =>
            new JsonValidationResult(JsonStatus.Valid, $"Valid JSON  •  {nodeCount} node{(nodeCount == 1 ? "" : "s")}", 0, 0, token);

        public static JsonValidationResult Warning(JToken token, string message) =>
            new JsonValidationResult(JsonStatus.Warning, message, 0, 0, token);

        public static JsonValidationResult Invalid(string message, int line, int column) =>
            new JsonValidationResult(JsonStatus.Invalid, message, line, column, null);
    }

    /// <summary>
    /// Editor-only JSON parse / validate / reformat helpers used by the JSON editor window and drawer.
    /// Dates are kept as raw strings (DateParseHandling.None) so a date-like value is not rewritten just by
    /// viewing it, trailing (non-comment) content after the value is reported as an error, and non-finite
    /// numbers (NaN / Infinity) are rejected as invalid. Numbers are parsed as double, so Beautify/Minify may
    /// normalize numeric formatting (e.g. 1.50 -> 1.5); values beyond double precision are not preserved.
    /// </summary>
    internal static class JsonValidation
    {
        /// <summary>
        /// Parses <paramref name="text"/> into a single <see cref="JToken"/>. Returns false with a
        /// human-readable <paramref name="error"/> (and 1-based <paramref name="line"/>/<paramref name="col"/>
        /// when available) on any problem. Whitespace-only input is treated as a parse failure here - callers
        /// that treat empty as valid check that first (see <see cref="Validate"/>).
        /// </summary>
        public static bool TryParse(string text, out JToken token, out string error, out int line, out int col)
        {
            token = null;
            error = null;
            line = 0;
            col = 0;

            if (string.IsNullOrWhiteSpace(text))
            {
                error = "Empty.";
                return false;
            }

            try
            {
                using StringReader sr = new StringReader(text);
                using JsonTextReader reader = new JsonTextReader(sr)
                {
                    // Keep dates as authored (don't turn date-like strings into DateTime tokens on load).
                    DateParseHandling = DateParseHandling.None,
                    FloatParseHandling = FloatParseHandling.Double,
                };

                JsonLoadSettings settings = new JsonLoadSettings
                {
                    CommentHandling = CommentHandling.Ignore,
                    LineInfoHandling = LineInfoHandling.Ignore,
                    DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Replace,
                };

                token = JToken.Load(reader, settings);

                // JToken.Load reads exactly one value; anything after it (a second value, stray characters)
                // is malformed input the user should know about. Trailing comments are tolerated to match the
                // CommentHandling.Ignore intent (leading/interior comments already parse).
                while (reader.Read())
                {
                    if (reader.TokenType == JsonToken.Comment)
                        continue;

                    line = reader.LineNumber;
                    col = reader.LinePosition;
                    error = "Unexpected trailing content after the JSON value.";
                    token = null;
                    return false;
                }

                return true;
            }
            catch (JsonReaderException e)
            {
                error = CleanMessage(e.Message);
                line = e.LineNumber;
                col = e.LinePosition;
                token = null;
                return false;
            }
            catch (Exception e)
            {
                error = CleanMessage(e.Message);
                token = null;
                return false;
            }
        }

        /// <summary>Full classification for status UI: Empty / Valid / Warning / Invalid.</summary>
        public static JsonValidationResult Validate(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return JsonValidationResult.Empty();

            if (!TryParse(text, out JToken token, out string error, out int line, out int col))
                return JsonValidationResult.Invalid(error, line, col);

            // NaN / Infinity / overflowing exponents parse (Newtonsoft is lenient) but re-serialize to the
            // JSON STRINGS "NaN"/"Infinity" - a silent number->string type change on Beautify/Minify. Reject
            // them so the status is honest and Apply is blocked.
            if (HasNonFiniteNumber(token))
                return JsonValidationResult.Invalid("Contains a non-finite number (NaN or Infinity), which is not valid JSON.", 0, 0);

            string duplicateKey = FindDuplicateKey(text);
            if (duplicateKey != null)
                return JsonValidationResult.Warning(token, $"Valid, but duplicate key \"{duplicateKey}\" found - the last value wins.");

            return JsonValidationResult.Valid(token, CountNodes(token));
        }

        /// <summary>Pretty-prints valid JSON; returns null and sets <paramref name="error"/> if invalid.</summary>
        public static string Beautify(string text, out string error)
        {
            if (!TryParse(text, out JToken token, out error, out _, out _))
                return null;
            return token.ToString(Formatting.Indented);
        }

        /// <summary>Collapses valid JSON to one line; returns null and sets <paramref name="error"/> if invalid.</summary>
        public static string Minify(string text, out string error)
        {
            if (!TryParse(text, out JToken token, out error, out _, out _))
                return null;
            return token.ToString(Formatting.None);
        }

        // Re-parses strictly to detect duplicate object keys (Newtonsoft's default is lenient and keeps the
        // last one). Returns the offending key name, or null when there are no duplicates.
        private static string FindDuplicateKey(string text)
        {
            try
            {
                using StringReader sr = new StringReader(text);
                using JsonTextReader reader = new JsonTextReader(sr) { DateParseHandling = DateParseHandling.None };
                JToken.Load(reader, new JsonLoadSettings
                {
                    CommentHandling = CommentHandling.Ignore,
                    DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Error,
                });
                return null;
            }
            catch (JsonReaderException e)
            {
                // Message shape: "Property with the name 'X' already exists in the current JSON object. ..."
                return ExtractDuplicateKey(e.Message) ?? "duplicate";
            }
            catch
            {
                return null;
            }
        }

        // True if any number in the tree is a non-finite double (NaN / +/-Infinity, incl. exponent overflow).
        private static bool HasNonFiniteNumber(JToken token)
        {
            switch (token)
            {
                case JObject obj:
                    foreach (JProperty p in obj.Properties())
                    {
                        if (HasNonFiniteNumber(p.Value))
                            return true;
                    }

                    return false;
                case JArray arr:
                    foreach (JToken item in arr)
                    {
                        if (HasNonFiniteNumber(item))
                            return true;
                    }

                    return false;
                case JValue value when value.Type == JTokenType.Float:
                    if (value.Value is double d)
                        return double.IsNaN(d) || double.IsInfinity(d);
                    if (value.Value is float f)
                        return float.IsNaN(f) || float.IsInfinity(f);
                    return false;
                default:
                    return false;
            }
        }

        // Total node count (this token + all descendants), for the "N nodes" status readout.
        public static int CountNodes(JToken token)
        {
            int count = 1;
            switch (token)
            {
                case JObject obj:
                    foreach (JProperty p in obj.Properties())
                        count += CountNodes(p.Value);
                    break;
                case JArray arr:
                    foreach (JToken item in arr)
                        count += CountNodes(item);
                    break;
            }

            return count;
        }

        // Pulls the duplicate key name out of a Newtonsoft message. The format is stable:
        // "Property with the name '<key>' already exists in the current JSON object. ...". Anchor on the
        // "' already exists" suffix (not the next quote) so a key that itself contains a single quote is not
        // truncated; fall back to the next quote if the suffix is absent.
        private static string ExtractDuplicateKey(string message)
        {
            if (string.IsNullOrEmpty(message))
                return null;

            int start = message.IndexOf('\'');
            if (start < 0)
                return null;

            int end = message.IndexOf("' already exists", start + 1, StringComparison.Ordinal);
            if (end < 0)
                end = message.IndexOf('\'', start + 1);
            if (end < 0)
                return null;

            return message.Substring(start + 1, end - start - 1);
        }

        // Newtonsoft appends "Path '...', line L, position P." to messages; we surface line/position
        // separately in the UI, so trim that tail to keep the headline message short.
        private static string CleanMessage(string message)
        {
            if (string.IsNullOrEmpty(message))
                return "Invalid JSON.";

            int idx = message.IndexOf(" Path '", StringComparison.Ordinal);
            return idx > 0 ? message.Substring(0, idx) : message;
        }
    }
}
