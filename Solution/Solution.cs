namespace Solution;

/// <summary>
/// Delegato generico, consente di referenziare metodi con parametri varibili non definiti.
/// </summary>
/// <param name="aparam">Lista di parametri</param>
public delegate void ParamEventHandler(params object[] aparam);
