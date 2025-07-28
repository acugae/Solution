using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Solution.SolutionMapper.Converters;

namespace Solution.SolutionMapper.Extensions;

/// <summary>
/// Classe statica di estensione per configurare e registrare SolutionMapper nei servizi DI.
/// Permette di aggiungere mapping e converter di base e di caricare i profili di mapping personalizzati.
/// </summary>
public static class SolutionMapperConfiguration
{
    /// <summary>
    /// Registra SolutionMapper come singleton nei servizi DI e configura i converter di base e i profili di mapping.
    /// </summary>
    /// <param name="services">Collezione dei servizi DI</param>
    /// <param name="assembly">Assembly da cui caricare i profili di mapping</param>
    /// <returns>La collezione dei servizi aggiornata</returns>
    public static IServiceCollection AddSolutionMapper(
        this IServiceCollection  services,
        Assembly assembly)
    {
        // Crea una nuova istanza di SolutionMapper e la relativa configurazione
        var mapper = new SolutionMapper();
        var cfg = new SolutionMapperConfigurationExpression(mapper);

        // Registrazione dei converter di base per i tipi comuni
        cfg.CreateMap<long?, string>().ConvertUsing(new ConvertersBase.NullableLongToStringConverter());
        cfg.CreateMap<int?, string>().ConvertUsing(new ConvertersBase.NullableIntToStringConverter());
        cfg.CreateMap<int, string>().ConvertUsing(new ConvertersBase.IntToStringConverter());
        cfg.CreateMap<long, string>().ConvertUsing(new ConvertersBase.LongToStringConverter());
        cfg.CreateMap<string, int>().ConvertUsing(new ConvertersBase.StringToIntConverter());
        cfg.CreateMap<string, long>().ConvertUsing(new ConvertersBase.StringToLongConverter());
        cfg.CreateMap<string, int?>().ConvertUsing(new ConvertersBase.StringToNullableIntConverter());
        cfg.CreateMap<string, long?>().ConvertUsing(new ConvertersBase.StringToNullableLongConverter());
        cfg.CreateMap<decimal, string>().ConvertUsing(new ConvertersBase.DecimalToStringConverter());
        cfg.CreateMap<decimal?, string>().ConvertUsing(new ConvertersBase.NullableDecimalToStringConverter());
        cfg.CreateMap<string, decimal>().ConvertUsing(new ConvertersBase.StringToDecimalConverter());
        cfg.CreateMap<string, decimal?>().ConvertUsing(new ConvertersBase.StringToNullableDecimalConverter());
        cfg.CreateMap<string, DateTime>().ConvertUsing(new ConvertersBase.StringToDateTimeConverter());
        cfg.CreateMap<string, DateTime?>().ConvertUsing(new ConvertersBase.StringToNullableDateTimeConverter());
        cfg.CreateMap<DateTime, string>().ConvertUsing(new ConvertersBase.DateToStringConverter());
        cfg.CreateMap<DateTime?, string>().ConvertUsing(new ConvertersBase.NullableDateToStringConverter());
        cfg.CreateMap<bool, string>().ConvertUsing(new ConvertersBase.BoolToString());
        cfg.CreateMap<bool?, string>().ConvertUsing(new ConvertersBase.NullableBoolToString());
        cfg.CreateMap<string, bool>().ConvertUsing(new ConvertersBase.StringToBool());
        cfg.CreateMap<string, bool?>().ConvertUsing(new ConvertersBase.StringToNullableBool());
        cfg.CreateMap<string, IList<int>>().ConvertUsing(new ConvertersBase.StringToListOfInt());
        cfg.CreateMap<IList<int>, string>().ConvertUsing(new ConvertersBase.ListOfIntToString());
        cfg.CreateMap<string, IList<long>>().ConvertUsing(new ConvertersBase.StringToListOfLong());
        cfg.CreateMap<IList<long>, string>().ConvertUsing(new ConvertersBase.ListOfLongToString());
        cfg.CreateMap<string, IList<string>>().ConvertUsing(new ConvertersBase.StringToListOfString());

        // Carica e registra tutti i profili di mapping definiti nell'assembly
        var profileTypes = assembly.GetTypes()
            .Where(t => typeof(SolutionMapperProfile).IsAssignableFrom(t) && !t.IsAbstract && t.IsClass);

        foreach (var type in profileTypes)
        {
            // Istanzia il profilo passando il mapper e lo registra
            var profile = (SolutionMapperProfile)Activator.CreateInstance(type, mapper);
            mapper.AddProfile(profile);
        }

        // Registra il mapper come singleton nei servizi DI
        services.AddSingleton(mapper);
        return services;
    }
}