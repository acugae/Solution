using System.Linq.Expressions;

namespace Solution.SolutionMapper;

/// <summary>
/// Interfaccia fluente per la configurazione delle regole di mapping tra tipi sorgente e destinazione.
/// Permette di personalizzare la mappatura di proprietà, costruttori, azioni, converter e altre opzioni.
/// </summary>
public interface ISolutionMappingExpression<TSource, TDest>
{
    /// <summary>
    /// Configura il mapping tra una proprietà di destinazione e una di sorgente, con eventuale converter.
    /// </summary>
    /// <typeparam name="TMember">Tipo della proprietà mappata</typeparam>
    /// <param name="destSelector">Espressione per selezionare la proprietà di destinazione</param>
    /// <param name="srcSelector">Espressione per selezionare la proprietà di sorgente</param>
    /// <param name="converter">Converter opzionale per la trasformazione del valore</param>
    ISolutionMappingExpression<TSource, TDest> ForMember<TMember>(
        Expression<Func<TDest, TMember>> destSelector,
        Expression<Func<TSource, TMember>> srcSelector,
        IValueConverter<TMember, TMember> converter = null);

    /// <summary>
    /// Configura opzioni avanzate per un membro sorgente specifico.
    /// </summary>
    /// <param name="sourceMemberName">Nome del membro sorgente</param>
    /// <param name="memberOptions">Azione di configurazione per il membro</param>
    ISolutionMappingExpression<TSource, TDest> ForSourceMember(string sourceMemberName, Action<ISourceMemberConfigurationExpression> memberOptions);

    /// <summary>
    /// Ignora una proprietà di destinazione specificata tramite espressione.
    /// </summary>
    /// <param name="destSelector">Espressione per selezionare la proprietà da ignorare</param>
    ISolutionMappingExpression<TSource, TDest> Ignore(Expression<Func<TDest, object>> destSelector);

    /// <summary>
    /// Ignora una proprietà di destinazione specificata per nome.
    /// </summary>
    /// <param name="field">Nome della proprietà da ignorare</param>
    ISolutionMappingExpression<TSource, TDest> Ignore(string field);

    /// <summary>
    /// Imposta un'azione da eseguire prima del mapping.
    /// </summary>
    /// <param name="beforeAction">Azione da eseguire</param>
    ISolutionMappingExpression<TSource, TDest> BeforeMap(Action<TSource, TDest> beforeAction);

    /// <summary>
    /// Imposta un'azione da eseguire prima del mapping, con accesso al contesto di risoluzione.
    /// </summary>
    /// <param name="beforeAction">Azione da eseguire</param>
    ISolutionMappingExpression<TSource, TDest> BeforeMap(Action<TSource, TDest, ResolutionContext> beforeAction);

    /// <summary>
    /// Imposta una classe di azione personalizzata da eseguire prima del mapping.
    /// </summary>
    /// <typeparam name="TMappingAction">Tipo dell'azione di mapping</typeparam>
    ISolutionMappingExpression<TSource, TDest> BeforeMap<TMappingAction>() where TMappingAction : IMappingAction<TSource, TDest>, new();

    /// <summary>
    /// Imposta un'azione da eseguire dopo il mapping.
    /// </summary>
    /// <param name="afterAction">Azione da eseguire</param>
    ISolutionMappingExpression<TSource, TDest> AfterMap(Action<TSource, TDest> afterAction);

    /// <summary>
    /// Imposta un'azione da eseguire dopo il mapping, con accesso al contesto di risoluzione.
    /// </summary>
    /// <param name="afterAction">Azione da eseguire</param>
    ISolutionMappingExpression<TSource, TDest> AfterMap(Action<TSource, TDest, ResolutionContext> afterAction);

    /// <summary>
    /// Imposta una classe di azione personalizzata da eseguire dopo il mapping.
    /// </summary>
    /// <typeparam name="TMappingAction">Tipo dell'azione di mapping</typeparam>
    ISolutionMappingExpression<TSource, TDest> AfterMap<TMappingAction>() where TMappingAction : IMappingAction<TSource, TDest>, new();

    /// <summary>
    /// Usa un converter personalizzato per la mappatura tra TSource e TDest.
    /// </summary>
    /// <param name="converter">Istanza del converter</param>
    ISolutionMappingExpression<TSource, TDest> ConvertUsing(ITypeConverter<TSource, TDest> converter);

    /// <summary>
    /// Usa una funzione personalizzata per la mappatura tra TSource e TDest.
    /// </summary>
    /// <param name="mappingFunction">Funzione di mapping</param>
    ISolutionMappingExpression<TSource, TDest> ConvertUsing(Func<TSource, TDest, ResolutionContext, TDest> mappingFunction);

    /// <summary>
    /// Usa un'espressione personalizzata per la mappatura tra TSource e TDest.
    /// </summary>
    /// <param name="mappingExpression">Espressione di mapping</param>
    ISolutionMappingExpression<TSource, TDest> ConvertUsing(Expression<Func<TSource, TDest>> mappingExpression);

    /// <summary>
    /// Usa un costruttore personalizzato per creare l'oggetto di destinazione.
    /// </summary>
    /// <param name="ctor">Funzione costruttore</param>
    ISolutionMappingExpression<TSource, TDest> ConstructUsing(Func<TSource, TDest> ctor);

    /// <summary>
    /// Usa un costruttore personalizzato con contesto per creare l'oggetto di destinazione.
    /// </summary>
    /// <param name="ctor">Funzione costruttore con contesto</param>
    ISolutionMappingExpression<TSource, TDest> ConstructUsing(Func<TSource, ResolutionContext, TDest> ctor);

    /// <summary>
    /// Imposta la profondità massima per il mapping ricorsivo.
    /// </summary>
    /// <param name="depth">Valore della profondità massima</param>
    ISolutionMappingExpression<TSource, TDest> MaxDepth(int depth);

    /// <summary>
    /// Abilita la preservazione dei riferimenti durante il mapping.
    /// </summary>
    ISolutionMappingExpression<TSource, TDest> PreserveReferences();

    /// <summary>
    /// Disabilita la validazione del costruttore.
    /// </summary>
    ISolutionMappingExpression<TSource, TDest> DisableCtorValidation();

    /// <summary>
    /// Imposta la modalità di validazione dei membri (sorgente, destinazione o nessuna).
    /// </summary>
    /// <param name="memberList">Tipo di validazione</param>
    ISolutionMappingExpression<TSource, TDest> ValidateMemberList(MemberList memberList);

    /// <summary>
    /// Include tutti i tipi derivati nel mapping.
    /// </summary>
    ISolutionMappingExpression<TSource, TDest> IncludeAllDerived();

    /// <summary>
    /// Include un tipo derivato specifico nel mapping.
    /// </summary>
    /// <param name="derivedSourceType">Tipo sorgente derivato</param>
    /// <param name="derivedDestinationType">Tipo destinazione derivato</param>
    ISolutionMappingExpression<TSource, TDest> Include(Type derivedSourceType, Type derivedDestinationType);

    /// <summary>
    /// Include un tipo base specifico nel mapping.
    /// </summary>
    /// <param name="sourceBase">Tipo base sorgente</param>
    /// <param name="destinationBase">Tipo base destinazione</param>
    ISolutionMappingExpression<TSource, TDest> IncludeBase(Type sourceBase, Type destinationBase);

    /// <summary>
    /// Ignora tutte le proprietà di destinazione con setter non accessibili.
    /// </summary>
    ISolutionMappingExpression<TSource, TDest> IgnoreAllPropertiesWithAnInaccessibleSetter();

    /// <summary>
    /// Ignora tutte le proprietà sorgente con setter non accessibili.
    /// </summary>
    ISolutionMappingExpression<TSource, TDest> IgnoreAllSourcePropertiesWithAnInaccessibleSetter();

    /// <summary>
    /// Configura le opzioni per un parametro del costruttore.
    /// </summary>
    /// <param name="ctorParamName">Nome del parametro del costruttore</param>
    /// <param name="paramOptions">Azione di configurazione</param>
    ISolutionMappingExpression<TSource, TDest> ForCtorParam(string ctorParamName, Action<ICtorParamConfigurationExpression<TSource>> paramOptions);

    /// <summary>
    /// Collezione di trasformatori di valore da applicare durante il mapping.
    /// </summary>
    IList<ValueTransformerConfiguration> ValueTransformers { get; }

    /// <summary>
    /// Crea una configurazione inversa di mapping (ReverseMap) e la aggiunge al profilo specificato.
    /// </summary>
    /// <param name="profile">Profilo di mapping in cui aggiungere la configurazione inversa</param>
    /// <returns>Espressione di mapping inversa tra TDest e TSource</returns>
    SolutionMappingExpression<TDest, TSource> ReverseMap(SolutionMapperProfile profile);
}