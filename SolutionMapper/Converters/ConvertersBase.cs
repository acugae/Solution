using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using Newtonsoft.Json;
namespace Solution.SolutionMapper.Converters;

/// <summary>
/// Raccolta di converter di base per la trasformazione tra tipi comuni.
/// Ogni classe implementa le interfacce IValueConverter e ITypeConverter per gestire la conversione tra tipi specifici.
/// </summary>
public static class ConvertersBase
{
    /// <summary>
    /// Converte una stringa in intero. Se la conversione fallisce, restituisce 0.
    /// </summary>
    public class StringToIntConverter : IValueConverter<string, int>, ITypeConverter<string, int>
    {
        public int Convert(string source, int destination, ResolutionContext context)
        {
            destination = Convert(source, context);
            return destination;
        }

        public int Convert(string sourceMember, ResolutionContext context)
        {
            var res = 0;
            if (int.TryParse(sourceMember, out var result)) res = result;
            return res;
        }
    }

    /// <summary>
    /// Arrotonda un valore decimale nullable al numero di decimali specificato.
    /// </summary>
    public class NullableDecimalRounded : IValueConverter<decimal?, decimal?>, ITypeConverter<decimal?, decimal?>
    {
        private int? _n = null;

        public NullableDecimalRounded() { }

        public NullableDecimalRounded(int n)
        {
            _n = n;
        }

        public decimal? Convert(decimal? source, decimal? destination, ResolutionContext context)
        {
            destination = Convert(source, context);
            return destination;
        }

        public decimal? Convert(decimal? sourceMember, ResolutionContext context)
        {
            var res = (decimal?)null;
            if (sourceMember.HasValue && _n.HasValue)
            {
                res = Math.Round(sourceMember.Value, _n.Value);
            }
            else if (sourceMember.HasValue && !_n.HasValue)
            {
                res = sourceMember;
            }
            return res;
        }
    }

    /// <summary>
    /// Arrotonda un valore decimale al numero di decimali specificato.
    /// </summary>
    public class DecimalRounded : IValueConverter<decimal, decimal>, ITypeConverter<decimal, decimal>
    {
        private int? _n = null;

        public DecimalRounded() { }

        public DecimalRounded(int n)
        {
            _n = n;
        }

        public decimal Convert(decimal source, decimal destination, ResolutionContext context)
        {
            destination = Convert(source, context);
            return destination;
        }

        public decimal Convert(decimal sourceMember, ResolutionContext context)
        {
            return Math.Round(sourceMember, _n.Value);
        }
    }

    /// <summary>
    /// Converte una stringa in intero nullable. Se la conversione fallisce, restituisce null.
    /// </summary>
    public class StringToNullableIntConverter : IValueConverter<string, int?>, ITypeConverter<string, int?>
    {
        public int? Convert(string source, int? destination, ResolutionContext context)
        {
            destination = Convert(source, context);
            return destination;
        }

        public int? Convert(string sourceMember, ResolutionContext context)
        {
            int? res = null;
            if (int.TryParse(sourceMember, out var result)) res = result;
            return res;
        }
    }

    /// <summary>
    /// Converte una stringa in una lista di interi, separando i valori tramite virgola.
    /// </summary>
    public class StringToListOfInt : IValueConverter<string, IList<int>>, ITypeConverter<string, IList<int>>
    {
        public IList<int> Convert(string source, IList<int> destination, ResolutionContext context)
        {
            destination = Convert(source, context);
            return destination;
        }

        public IList<int> Convert(string sourceMember, ResolutionContext context)
        {
            return !string.IsNullOrEmpty(sourceMember)
                ? sourceMember.Trim().Split(",").Select(s =>
                {
                    if (int.TryParse(s, out var r))
                        return (int?)r;
                    return (int?)null;
                })
                    .Where(p => p.HasValue)
                    .Cast<int>().ToList()
                : new List<int>();
        }
    }

    /// <summary>
    /// Converte una stringa in una lista di long, separando i valori tramite virgola.
    /// </summary>
    public class StringToListOfLong : IValueConverter<string, IList<long>>, ITypeConverter<string, IList<long>>
    {
        public IList<long> Convert(string source, IList<long> destination, ResolutionContext context)
        {
            destination = Convert(source, context);
            return destination;
        }

        public IList<long> Convert(string sourceMember, ResolutionContext context)
        {
            return !string.IsNullOrEmpty(sourceMember)
                ? sourceMember.Trim().Split(",").Select(s =>
                {
                    if (long.TryParse(s, out var r))
                        return (long?)r;
                    return (long?)null;
                })
                    .Where(p => p.HasValue)
                    .Cast<long>().ToList()
                : new List<long>();
        }
    }

    /// <summary>
    /// Converte una stringa in una lista di stringhe, separando i valori tramite virgola.
    /// </summary>
    public class StringToListOfString : IValueConverter<string, IList<string>>, ITypeConverter<string, IList<string>>
    {
        public IList<string> Convert(string sourceMember, ResolutionContext context)
        {
            return (!string.IsNullOrEmpty(sourceMember)
                ? sourceMember.Trim().Split(",")
                    .Select(s => s)
                    .Where(p => !string.IsNullOrEmpty(p))
                    .ToList()
                : new List<string>());
        }

        public IList<string> Convert(string source, IList<string> destination, ResolutionContext context)
        {
            destination = Convert(source, context);
            return destination;
        }
    }

    /// <summary>
    /// Converte una lista di interi in una stringa separata da virgole.
    /// </summary>
    public class ListOfIntToString : IValueConverter<IList<int>, string>, ITypeConverter<IList<int>, string>
    {
        public string Convert(IList<int> source, string destination, ResolutionContext context)
        {
            destination = Convert(source, context);
            return destination;
        }

        public string Convert(IList<int> sourceMember, ResolutionContext context)
        {
            return string.Join(",", sourceMember);
        }
    }

    /// <summary>
    /// Converte una lista di long in una stringa separata da virgole.
    /// </summary>
    public class ListOfLongToString : IValueConverter<IList<long>, string>, ITypeConverter<IList<long>, string>
    {
        public string Convert(IList<long> source, string destination, ResolutionContext context)
        {
            destination = Convert(source, context);
            return destination;
        }

        public string Convert(IList<long> sourceMember, ResolutionContext context)
        {
            return string.Join(",", sourceMember);
        }
    }

    /// <summary>
    /// Converte una stringa in long. Se la conversione fallisce, restituisce 0.
    /// </summary>
    public class StringToLongConverter : IValueConverter<string, long>, ITypeConverter<string, long>
    {
        public long Convert(string source, long destination, ResolutionContext context)
        {
            destination = Convert(source, context);
            return destination;
        }

        public long Convert(string sourceMember, ResolutionContext context)
        {
            long res = 0;
            if (long.TryParse(sourceMember, out var result)) res = result;
            return res;
        }
    }

    /// <summary>
    /// Converte una stringa in long nullable. Se la conversione fallisce, restituisce null.
    /// </summary>
    public class StringToNullableLongConverter : IValueConverter<string, long?>, ITypeConverter<string, long?>
    {
        public long? Convert(string source, long? destination, ResolutionContext context)
        {
            destination = Convert(source, context);
            return destination;
        }

        public long? Convert(string sourceMember, ResolutionContext context)
        {
            long? res = null;
            if (long.TryParse(sourceMember, out var result)) res = result;
            return res;
        }
    }

    /// <summary>
    /// Converte un intero in stringa.
    /// </summary>
    public class IntToStringConverter : IValueConverter<int, string>, ITypeConverter<int, string>
    {
        public string Convert(int source, string destination, ResolutionContext context)
        {
            destination = Convert(source, context);
            return destination;
        }

        public string Convert(int sourceMember, ResolutionContext context)
        {
            return sourceMember.ToString();
        }
    }

    /// <summary>
    /// Converte un long in stringa.
    /// </summary>
    public class LongToStringConverter : IValueConverter<long, string>, ITypeConverter<long, string>
    {
        public string Convert(long source, string destination, ResolutionContext context)
        {
            destination = Convert(source, context);
            return destination;
        }

        public string Convert(long sourceMember, ResolutionContext context)
        {
            return sourceMember.ToString();
        }
    }

    /// <summary>
    /// Converte un intero nullable in stringa. Se non valorizzato, restituisce stringa vuota.
    /// </summary>
    public class NullableIntToStringConverter : IValueConverter<int?, string>, ITypeConverter<int?, string>
    {
        public string Convert(int? source, string destination, ResolutionContext context)
        {
            destination = Convert(source, context);
            return destination;
        }

        public string Convert(int? sourceMember, ResolutionContext context)
        {
            return sourceMember.HasValue ? sourceMember.Value.ToString() : "";
        }
    }

    /// <summary>
    /// Converte un long nullable in stringa. Se non valorizzato, restituisce stringa vuota.
    /// </summary>
    public class NullableLongToStringConverter : IValueConverter<long?, string>, ITypeConverter<long?, string>
    {
        public string Convert(long? source, string destination, ResolutionContext context)
        {
            destination = Convert(source, context);
            return destination;
        }

        public string Convert(long? sourceMember, ResolutionContext context)
        {
            return sourceMember.HasValue ? sourceMember.Value.ToString() : "";
        }
    }

    /// <summary>
    /// Converte una stringa in decimal nullable, gestendo la cultura corrente.
    /// </summary>
    public class StringToNullableDecimalConverter : IValueConverter<string, decimal?>, ITypeConverter<string, decimal?>
    {
        public decimal? Convert(string source, decimal? destination, ResolutionContext context)
        {
            destination = Convert(source, context);
            return destination;
        }

        public decimal? Convert(string sourceMember, ResolutionContext context)
        {
            try
            {
                if (string.IsNullOrEmpty(sourceMember) || string.IsNullOrWhiteSpace(sourceMember))
                    return null;
                try
                {
                    var culture = CultureInfo.CurrentCulture;
                    var sep = culture.NumberFormat.CurrencyDecimalSeparator;
                    var repIn = sep == "," ? "." : ",";
                    var repout = sep == "," ? "," : ".";
                    return decimal.Parse(sourceMember.Replace(repIn, repout), CultureInfo.CurrentCulture);
                }
                catch (Exception)
                {
                    throw new Exception("Impossibile convertire il valore in Double");
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    /// <summary>
    /// Converte un decimal nullable in stringa. Se non valorizzato, restituisce stringa vuota.
    /// </summary>
    public class NullableDecimalToStringConverter : IValueConverter<decimal?, string>, ITypeConverter<decimal?, string>
    {
        public string Convert(decimal? source, string destination, ResolutionContext context)
        {
            destination = Convert(source, context);
            return destination;
        }

        public string Convert(decimal? sourceMember, ResolutionContext context)
        {
            return sourceMember.HasValue ? sourceMember.Value.ToString() : "";
        }
    }

    /// <summary>
    /// Converte una stringa in decimal, gestendo la cultura corrente.
    /// </summary>
    public class StringToDecimalConverter : IValueConverter<string, decimal>, ITypeConverter<string, decimal>
    {
        public decimal Convert(string source, decimal destination, ResolutionContext context)
        {
            destination = Convert(source, context);
            return destination;
        }

        public decimal Convert(string sourceMember, ResolutionContext context)
        {
            try
            {
                try
                {
                    if (string.IsNullOrEmpty(sourceMember) || string.IsNullOrWhiteSpace(sourceMember))
                        return 0;
                    try
                    {
                        var culture = CultureInfo.CurrentCulture;
                        var sep = culture.NumberFormat.CurrencyDecimalSeparator;
                        var repIn = sep == "," ? "." : ",";
                        var repout = sep == "," ? "," : ".";
                        return decimal.Parse(sourceMember.Replace(repIn, repout), CultureInfo.CurrentCulture);
                    }
                    catch (Exception)
                    {
                        throw new Exception("Impossibile convertire il valore in Double");
                    }
                }
                catch (Exception)
                {
                    return 0;
                }
            }
            catch
            {
                return 0;
            }
        }
    }

    /// <summary>
    /// Converte un decimal in stringa.
    /// </summary>
    public class DecimalToStringConverter : IValueConverter<decimal, string>, ITypeConverter<decimal, string>
    {
        public string Convert(decimal source, string destination, ResolutionContext context)
        {
            destination = Convert(source, context);
            return destination;
        }

        public string Convert(decimal sourceMember, ResolutionContext context)
        {
            return sourceMember.ToString();
        }
    }

    /// <summary>
    /// Converte una stringa in DateTime usando la cultura italiana. Se fallisce, restituisce un valore di default.
    /// </summary>
    public class StringToDateTimeConverter : IValueConverter<string, DateTime>, ITypeConverter<string, DateTime>
    {
        public DateTime Convert(string source, DateTime destination, ResolutionContext context)
        {
            destination = Convert(source, context);
            return destination;
        }

        public DateTime Convert(string sourceMember, ResolutionContext context)
        {
            return !string.IsNullOrEmpty(sourceMember) && DateTime.TryParse(sourceMember,
                CultureInfo.GetCultureInfo("it-IT"), DateTimeStyles.None,
                out var requestDate)
                ? requestDate
                : new DateTime();
        }
    }

    /// <summary>
    /// Converte una stringa in DateTime nullable usando la cultura italiana. Se fallisce, restituisce null.
    /// </summary>
    public class StringToNullableDateTimeConverter : IValueConverter<string, DateTime?>, ITypeConverter<string, DateTime?>
    {
        public DateTime? Convert(string source, DateTime? destination, ResolutionContext context)
        {
            destination = Convert(source, context);
            return destination;
        }

        public DateTime? Convert(string sourceMember, ResolutionContext context)
        {
            return !string.IsNullOrEmpty(sourceMember) && DateTime.TryParse(sourceMember,
                CultureInfo.GetCultureInfo("it-IT"), DateTimeStyles.None,
                out var requestDate)
                ? requestDate
                : (DateTime?)null;
        }
    }

    /// <summary>
    /// Converte un DateTime in stringa con formato "dd/MM/yyyy".
    /// </summary>
    public class DateToStringConverter : IValueConverter<DateTime, string>, ITypeConverter<DateTime, string>
    {
        public string Convert(DateTime source, string destination, ResolutionContext context)
        {
            destination = Convert(source, context);
            return destination;
        }

        public string Convert(DateTime sourceMember, ResolutionContext context)
        {
            return sourceMember.ToString("dd/MM/yyyy");
        }
    }

    /// <summary>
    /// Converte un DateTime nullable in stringa con formato "dd/MM/yyyy". Se non valorizzato, restituisce stringa vuota.
    /// </summary>
    public class NullableDateToStringConverter : IValueConverter<DateTime?, string>, ITypeConverter<DateTime?, string>
    {
        public string Convert(DateTime? source, string destination, ResolutionContext context)
        {
            destination = Convert(source, context);
            return destination;
        }

        public string Convert(DateTime? sourceMember, ResolutionContext context)
        {
            return sourceMember.HasValue ? sourceMember.Value.ToString("dd/MM/yyyy") : "";
        }
    }

    /// <summary>
    /// Converte un DateTime in stringa con formato "dd/MM/yyyy HH:mm:ss".
    /// </summary>
    public class DateAndTimeToStringConverter : IValueConverter<DateTime, string>, ITypeConverter<DateTime, string>
    {
        public string Convert(DateTime source, string destination, ResolutionContext context)
        {
            destination = Convert(source, context);
            return destination;
        }

        public string Convert(DateTime sourceMember, ResolutionContext context)
        {
            return sourceMember.ToString("dd/MM/yyyy HH:mm:ss");
        }
    }

    /// <summary>
    /// Converte un DateTime nullable in stringa con formato "dd/MM/yyyy HH:mm:ss". Se non valorizzato, restituisce stringa vuota.
    /// </summary>
    public class NullableDateAndTimeToStringConverter : IValueConverter<DateTime?, string>, ITypeConverter<DateTime?, string>
    {
        public string Convert(DateTime? source, string destination, ResolutionContext context)
        {
            destination = Convert(source, context);
            return destination;
        }

        public string Convert(DateTime? sourceMember, ResolutionContext context)
        {
            return sourceMember.HasValue ? sourceMember.Value.ToString("dd/MM/yyyy HH:mm:ss") : "";
        }
    }

    /// <summary>
    /// Converte un intero nullable in intero. Se non valorizzato, restituisce 0.
    /// </summary>
    public class NullableIntToInt : IValueConverter<int?, int>, ITypeConverter<int?, int>
    {
        public int Convert(int? source, int destination, ResolutionContext context)
        {
            destination = Convert(source, context);
            return destination;
        }

        public int Convert(int? sourceMember, ResolutionContext context)
        {
            return sourceMember ?? 0;
        }
    }

    /// <summary>
    /// Converte un long nullable in long. Se non valorizzato, restituisce 0.
    /// </summary>
    public class NullableLongToLong : IValueConverter<long?, long>, ITypeConverter<long?, long>
    {
        public long Convert(long? source, long destination, ResolutionContext context)
        {
            destination = Convert(source, context);
            return destination;
        }

        public long Convert(long? sourceMember, ResolutionContext context)
        {
            return sourceMember ?? 0;
        }
    }

    /// <summary>
    /// Converte un booleano in stringa.
    /// </summary>
    public class BoolToString : IValueConverter<bool, string>, ITypeConverter<bool, string>
    {
        public string Convert(bool source, string destination, ResolutionContext context)
        {
            destination = Convert(source, context);
            return destination;
        }

        public string Convert(bool sourceMember, ResolutionContext context)
        {
            return sourceMember.ToString();
        }
    }

    /// <summary>
    /// Converte un booleano nullable in stringa.
    /// </summary>
    public class NullableBoolToString : IValueConverter<bool?, string>, ITypeConverter<bool?, string>
    {
        public string Convert(bool? source, string destination, ResolutionContext context)
        {
            destination = Convert(source, context);
            return destination;
        }

        public string Convert(bool? sourceMember, ResolutionContext context)
        {
            return sourceMember.ToString();
        }
    }

    /// <summary>
    /// Converte una stringa in booleano. Accetta "TRUE", "YES", "1", "-1" come true.
    /// </summary>
    public class StringToBool : IValueConverter<string, bool>, ITypeConverter<string, bool>
    {
        public bool Convert(string source, bool destination, ResolutionContext context)
        {
            destination = Convert(source, context);
            return destination;
        }

        public bool Convert(string sourceMember, ResolutionContext context)
        {
            if (string.IsNullOrEmpty(sourceMember))
                return false;
            switch ((sourceMember).Trim().ToUpper())
            {
                case "TRUE":
                case "YES":
                case "1":
                case "-1":
                    return true;
                default:
                    return false;
            }
        }
    }

    /// <summary>
    /// Converte una stringa in booleano nullable. Accetta "TRUE", "YES", "1", "-1" come true.
    /// </summary>
    public class StringToNullableBool : IValueConverter<string, bool?>, ITypeConverter<string, bool?>
    {
        public bool? Convert(string source, bool? destination, ResolutionContext context)
        {
            destination = Convert(source, context);
            return destination;
        }

        public bool? Convert(string sourceMember, ResolutionContext context)
        {
            if (string.IsNullOrEmpty(sourceMember))
                return null;
            switch (((string)sourceMember).Trim().ToUpper())
            {
                case "TRUE":
                case "YES":
                case "1":
                case "-1":
                    return true;
                default:
                    return false;
            }
        }
    }

    /// <summary>
    /// Converte una stringa vuota in stringa vuota, altrimenti restituisce la stringa originale.
    /// </summary>
    public class StringToEmptyString : IValueConverter<string, string>, ITypeConverter<string, string>
    {
        public string Convert(string source, string destination, ResolutionContext context)
        {
            destination = Convert(source, context);
            return destination;
        }

        public string Convert(string sourceMember, ResolutionContext context)
        {
            if (string.IsNullOrEmpty(sourceMember))
                return "";
            return sourceMember;
        }
    }

    /// <summary>
    /// Converte un DateTime nullable in stringa con formato invariato "yyyy-MM-ddTHH:mm:ss.fff". Se non valorizzato, restituisce stringa vuota.
    /// </summary>
    public class NullableDateTimeToStringInvariant : IValueConverter<DateTime?, string>, ITypeConverter<DateTime?, string>
    {
        public string Convert(DateTime? source, string destination, ResolutionContext context)
        {
            destination = Convert(source, context);
            return destination;
        }

        public string Convert(DateTime? sourceMember, ResolutionContext context)
        {
            return !sourceMember.HasValue ? "" : sourceMember.Value.ToString("yyyy-MM-ddTHH:mm:ss.fff");
        }
    }

    /// <summary>
    /// Converte un DateTime nullable in stringa con formato invariato "yyyy-MM-ddTHH:mm:ss.fff". Se non valorizzato, restituisce null.
    /// </summary>
    public class NullableDateTimeToStringInvariantNull : IValueConverter<DateTime?, string>, ITypeConverter<DateTime?, string>
    {
        public string Convert(DateTime? source, string destination, ResolutionContext context)
        {
            destination = Convert(source, context);
            return destination;
        }

        public string Convert(DateTime? sourceMember, ResolutionContext context)
        {
            return !sourceMember.HasValue ? null : sourceMember.Value.ToString("yyyy-MM-ddTHH:mm:ss.fff");
        }
    }

    /// <summary>
    /// Converte un DateTime nullable in stringa con formato Microsoft JSON. Se non valorizzato, restituisce null.
    /// </summary>
    public class NullableDateTimeToMicrosoftFormatStringNull : IValueConverter<DateTime?, string>, ITypeConverter<DateTime?, string>
    {
        public string Convert(DateTime? source, string destination, ResolutionContext context)
        {
            destination = Convert(source, context);
            return destination;
        }

        public string Convert(DateTime? sourceMember, ResolutionContext context)
        {
            return !sourceMember.HasValue ? null : JsonConvert.SerializeObject(sourceMember, new JsonSerializerSettings
            {
                DateFormatHandling = DateFormatHandling.MicrosoftDateFormat
            });
        }
    }

    /// <summary>
    /// Converte un DateTime nullable in stringa con formato Microsoft JSON. Se non valorizzato, restituisce stringa vuota.
    /// </summary>
    public class NullableDateTimeToMicrosoftFormatString : IValueConverter<DateTime?, string>, ITypeConverter<DateTime?, string>
    {
        public string Convert(DateTime? source, string destination, ResolutionContext context)
        {
            destination = Convert(source, context);
            return destination;
        }

        public string Convert(DateTime? sourceMember, ResolutionContext context)
        {
            return !sourceMember.HasValue ? "" : JsonConvert.SerializeObject(sourceMember, new JsonSerializerSettings
            {
                DateFormatHandling = DateFormatHandling.MicrosoftDateFormat
            });
        }
    }

    /// <summary>
    /// Converte un oggetto dinamico ExpandoObject in intero. Se la conversione fallisce, restituisce 0.
    /// </summary>
    public class DynamicToInt : IValueConverter<ExpandoObject, int>, ITypeConverter<ExpandoObject, int>
    {
        public int Convert(ExpandoObject source, int destination, ResolutionContext context)
        {
            destination = Convert(source, context);
            return destination;
        }

        public int Convert(ExpandoObject sourceMember, ResolutionContext context)
        {
            if (sourceMember != null && int.TryParse(sourceMember.ToString(), out var res))
                return res;
            return 0;
        }
    }

    /// <summary>
    /// Converte un oggetto dinamico ExpandoObject in intero nullable. Se la conversione fallisce, restituisce null.
    /// </summary>
    public class DynamicToNullableInt : IValueConverter<ExpandoObject, int?>, ITypeConverter<ExpandoObject, int?>
    {
        public int? Convert(ExpandoObject source, int? destination, ResolutionContext context)
        {
            destination = Convert(source, context);
            return destination;
        }

        public int? Convert(ExpandoObject sourceMember, ResolutionContext context)
        {
            if (sourceMember != null && int.TryParse(sourceMember.ToString(), out var res))
                return res;
            return null;
        }
    }

    /// <summary>
    /// Arrotonda un valore decimale al numero di decimali specificato. Se non specificato, restituisce il valore originale.
    /// </summary>
    public class DecimalToDecimalPointConverter : IValueConverter<decimal, decimal>, ITypeConverter<decimal, decimal>
    {
        private int? _n = null;

        public DecimalToDecimalPointConverter() { }

        public DecimalToDecimalPointConverter(int n)
        {
            this._n = n;
        }
        public decimal Convert(decimal sourceMember, ResolutionContext context)
        {
            if (_n.HasValue)
            {
                return Math.Round(sourceMember, _n.Value);
            }

            return sourceMember;
        }

        public decimal Convert(decimal source, decimal destination, ResolutionContext context)
        {
            destination = Convert(source, context);
            return destination;
        }
    }

    /// <summary>
    /// Arrotonda un valore decimale nullable al numero di decimali specificato. Se non specificato, restituisce il valore originale.
    /// </summary>
    public class NullableDecimalToDecimalPointConverter : IValueConverter<decimal?, decimal?>, ITypeConverter<decimal?, decimal?>
    {
        private int? _n = null;

        public NullableDecimalToDecimalPointConverter() { }

        public NullableDecimalToDecimalPointConverter(int n)
        {
            this._n = n;
        }
        public decimal? Convert(decimal? sourceMember, ResolutionContext context)
        {
            if (sourceMember.HasValue && _n.HasValue)
            {
                return Math.Round(sourceMember.Value, _n.Value);
            }

            return sourceMember;
        }

        public decimal? Convert(decimal? source, decimal? destination, ResolutionContext context)
        {
            destination = Convert(source, context);
            return destination;
        }
    }
}