namespace Solution;

/// <summary>
/// Classe contenente metodi per la gestione di stringhe.
/// </summary>
public class cString
{
    private string _string = "";
    private string vetNumeric = "0123456789";
    private string vetChar = "abcdefghilmnjkyxwopqrstuvzABCDEFGHILMNOPQRSTUVZXYWJK";
    private string vetVocali = "aeiouAEIOU";
    private string verSpecial = "*$-+?_&=!%{}/";
    /// <summary>
    /// Inizializza una nuova istanza della classe.
    /// </summary>
    public cString()
    {
    }
    /// <summary>
    /// Inizializza una nuova istanza della classe copiando il contenuto della stringa specificata.
    /// </summary>
    /// <param name="sString"></param>
    public cString(string sString)
    {
        _string = sString;
    }
    /// <summary>
    /// Preleva un numero di caratteri specificato partendo da sinistra.
    /// </summary>
    /// <param name="length">Numero di caratteri da prelevare</param>
    public string Left(int length)
    {
        if (length > _string.Length)
            return _string;
        string tmpstr = _string.Substring(0, length);
        return tmpstr;
    }
    /// <summary>
    /// Preleva un numero di caratteri specificato partendo da destra.
    /// </summary>
    /// <param name="length">Numero di caratteri da prelevare</param>
    public string Right(int length)
    {
        if (length > _string.Length)
            return _string;
        string tmpstr = _string.Substring(_string.Length - length, length);
        return tmpstr;
    }
    /// <summary>
    /// Sostituisce tutte le occorrenze di oldChar con newChar
    /// </summary>
    /// <param name="oldChar">Carattere da sostituire</param>
    /// <param name="newChar">Carattere con cui sostituire le occorrenze di oldChar</param>
    /// <returns>La stringa modificata</returns>
    public string Replace(char oldChar, char newChar)
    {
        string tmpstr = _string.Replace(oldChar, newChar);
        return tmpstr;
    }
    /// <summary>
    /// Sostituisce tutte le occorrenze di oldString con newString, consente la modalità (case insensitive) 
    /// </summary>
    /// <param name="oldString">Stringa da sostituire</param>
    /// <param name="newString">Stringa con cui sostituire le occorrenze di oldString</param>
    /// <returns>La stringa modificata</returns>
    public string Replace(string oldString, string newString, bool bIsCase)
    {
        if (bIsCase)
        {
            string tmpstr = _string.Replace(oldString, newString);
            return tmpstr;
        }
        else
        {
            string tmpstr = _string;
            int index = 0;
            int s = _string.ToLower().IndexOf(oldString.ToLower(), index);
            while (s >= 0)
            {
                oldString = _string.Substring(s, oldString.Length);
                tmpstr = tmpstr.Replace(oldString, newString);
                index += s + oldString.Length;
                if (index < _string.Length)
                {
                    s = _string.ToLower().IndexOf(oldString.ToLower(), index);
                }
                else
                {
                    s = -1;
                }
            }
            return tmpstr;
        }
    }
    /// <summary>
    /// Sostituisce tutte le occorrenze di StringSearch con (BeginTag + StringSearch + EndTag), consente la modalità (case insensitive) 
    /// </summary>
    /// <param name="StringSearch">Stringa da sostituire</param>
    /// <param name="BeginTag">Apertura del TAG sottoforma di Stringa con cui sostituire le occorrenze di StringSearch</param>
    /// <param name="EndTag">Chiusura del TAG sottoforma di Stringa con cui sostituire le occorrenze di StringSearch</param>
    /// <returns>La stringa modificata</returns>
    public string ReplaceTag(string StringSearch, string BeginTag, string EndTag)
    {
        string tmpstr = _string;
        int index = 0;
        int s = _string.ToLower().IndexOf(StringSearch.ToLower(), index);
        while (s >= 0)
        {
            if (s + StringSearch.Length > _string.Length)
                break;
            //
            string tmp = _string.Substring(s, StringSearch.Length);
            tmpstr = tmpstr.Replace(tmp, BeginTag + tmp + EndTag);
            index += s + StringSearch.Length;
            if (index < _string.Length - 1)
                s = _string.ToLower().IndexOf(StringSearch.ToLower(), index);
            else
                break;
        }
        return tmpstr;
    }
    /// <summary>
    /// Sostituisce la stringa contenuta tra startIndex e endIndex con la stringa newValue
    /// </summary>
    /// <param name="startIndex">Indice di partenza</param>
    /// <param name="endIndex">Indice di arrivo</param>
    /// <param name="newValue">Stringa con cui sostituire l'occorrenza</param>
    /// <returns>La stringa modificata</returns>
    public string Replace(int startIndex, int endIndex, string newValue)
    {
        string sTmpString = _string.Substring(startIndex, endIndex - startIndex + 1);
        return _string.Replace(sTmpString, newValue);
    }

    /// <summary>
    /// Restituisce una matrice di oggetti String contenente le sottostringhe delimitate dagli elementi di una matrice di oggetti Char specificata.
    /// </summary>
    /// <param name="separator">Array di separatori</param>
    /// <param name="cont">Numero massimo di sottostringhe</param>
    public string[] Split(char[] separator, int cont)
    {
        return _string.Split(separator, cont);
    }

    /// <summary>
    /// Restituisce una matrice di oggetti String contenente le sottostringhe delimitate dagli elementi di una matrice di oggetti Char specificata.
    /// </summary>
    /// <param name="separator">Array di separatori</param>
    public string[] Split(params char[] separator)
    {
        return _string.Split(separator);
    }
    /// <summary>
    /// Restituisce una matrice di oggetti String contenente le sottostringhe delimitate dalla stringa specificata.
    /// </summary>
    /// <param name="separator">Stringa separatrice</param>
    public string[] Split(string separator)
    {
        ArrayList oA = new ArrayList();
        int index = _string.IndexOf(separator);
        int prec = 0;
        for (; index >= 0;)
        {
            oA.Add(_string.Substring(prec, index - prec));
            prec = index + separator.Length;
            index = _string.IndexOf(separator, prec);
        }
        if (oA.Count == 0)
        {
            return null;
        }
        else
        {
            oA.Add(_string.Substring(prec, _string.Length - prec));
        }
        string[] oresult = new string[oA.Count];
        Array.Copy(oA.ToArray(), oresult, oresult.Length);
        return oresult;
    }
    /// <summary>
    /// Genera una stringa casuale con lunghezza minima specificata e lunghezza massima specificata.
    /// </summary>
    /// <param name="minLength">Munghezza minima della stringa.</param>
    /// <param name="maxLength">Munghezza massima della stringa.</param>
    /// <returns></returns>
    public string Generate(int minLength, int maxLength)
    {
        // Make sure that input parameters are valid.
        if (minLength <= 0 || maxLength <= 0 || minLength > maxLength)
            return null;

        // Create a local array containing supported password characters
        // grouped by types. You can remove character groups from this
        // array, but doing so will weaken the password strength.
        char[][] charGroups = new char[][]
        {
                vetChar.ToCharArray(),
                vetNumeric.ToCharArray(),
                verSpecial.ToCharArray()
        };

        // Use this array to track the number of unused characters in each
        // character group.
        int[] charsLeftInGroup = new int[charGroups.Length];

        // Initially, all characters in each group are not used.
        for (int i = 0; i < charsLeftInGroup.Length; i++)
            charsLeftInGroup[i] = charGroups[i].Length;

        // Use this array to track (iterate through) unused character groups.
        int[] leftGroupsOrder = new int[charGroups.Length];

        // Initially, all character groups are not used.
        for (int i = 0; i < leftGroupsOrder.Length; i++)
            leftGroupsOrder[i] = i;

        // Because we cannot use the default randomizer, which is based on the
        // current time (it will produce the same "random" number within a
        // second), we will use a random number generator to seed the
        // randomizer.

        // Use a 4-byte array to fill it with random bytes and convert it then
        // to an integer value.
        byte[] randomBytes = new byte[4];

        // Generate 4 random bytes.
        RandomNumberGenerator rng = RandomNumberGenerator.Create(); //  new RNGCryptoServiceProvider();
        rng.GetBytes(randomBytes);

        // Convert 4 bytes into a 32-bit integer value.
        int seed = (randomBytes[0] & 0x7f) << 24 |
                    randomBytes[1] << 16 |
                    randomBytes[2] << 8 |
                    randomBytes[3];

        // Now, this is real randomization.
        Random random = new Random(seed);

        // This array will hold password characters.
        char[] password = null;

        // Allocate appropriate memory for the password.
        if (minLength < maxLength)
            password = new char[random.Next(minLength, maxLength + 1)];
        else
            password = new char[minLength];

        // Index of the next character to be added to password.
        int nextCharIdx;

        // Index of the next character group to be processed.
        int nextGroupIdx;

        // Index which will be used to track not processed character groups.
        int nextLeftGroupsOrderIdx;

        // Index of the last non-processed character in a group.
        int lastCharIdx;

        // Index of the last non-processed group.
        int lastLeftGroupsOrderIdx = leftGroupsOrder.Length - 1;

        // Generate password characters one at a time.
        for (int i = 0; i < password.Length; i++)
        {
            // If only one character group remained unprocessed, process it;
            // otherwise, pick a random character group from the unprocessed
            // group list. To allow a special character to appear in the
            // first position, increment the second parameter of the Next
            // function call by one, i.e. lastLeftGroupsOrderIdx + 1.
            if (lastLeftGroupsOrderIdx == 0)
                nextLeftGroupsOrderIdx = 0;
            else
                nextLeftGroupsOrderIdx = random.Next(0,
                                                     lastLeftGroupsOrderIdx);

            // Get the actual index of the character group, from which we will
            // pick the next character.
            nextGroupIdx = leftGroupsOrder[nextLeftGroupsOrderIdx];

            // Get the index of the last unprocessed characters in this group.
            lastCharIdx = charsLeftInGroup[nextGroupIdx] - 1;

            // If only one unprocessed character is left, pick it; otherwise,
            // get a random character from the unused character list.
            if (lastCharIdx == 0)
                nextCharIdx = 0;
            else
                nextCharIdx = random.Next(0, lastCharIdx + 1);

            // Add this character to the password.
            password[i] = charGroups[nextGroupIdx][nextCharIdx];

            // If we processed the last character in this group, start over.
            if (lastCharIdx == 0)
                charsLeftInGroup[nextGroupIdx] =
                                          charGroups[nextGroupIdx].Length;
            // There are more unprocessed characters left.
            else
            {
                // Swap processed character with the last unprocessed character
                // so that we don't pick it until we process all characters in
                // this group.
                if (lastCharIdx != nextCharIdx)
                {
                    char temp = charGroups[nextGroupIdx][lastCharIdx];
                    charGroups[nextGroupIdx][lastCharIdx] =
                                charGroups[nextGroupIdx][nextCharIdx];
                    charGroups[nextGroupIdx][nextCharIdx] = temp;
                }
                // Decrement the number of unprocessed characters in
                // this group.
                charsLeftInGroup[nextGroupIdx]--;
            }

            // If we processed the last group, start all over.
            if (lastLeftGroupsOrderIdx == 0)
                lastLeftGroupsOrderIdx = leftGroupsOrder.Length - 1;
            // There are more unprocessed groups left.
            else
            {
                // Swap processed group with the last unprocessed group
                // so that we don't pick it until we process all groups.
                if (lastLeftGroupsOrderIdx != nextLeftGroupsOrderIdx)
                {
                    int temp = leftGroupsOrder[lastLeftGroupsOrderIdx];
                    leftGroupsOrder[lastLeftGroupsOrderIdx] =
                                leftGroupsOrder[nextLeftGroupsOrderIdx];
                    leftGroupsOrder[nextLeftGroupsOrderIdx] = temp;
                }
                // Decrement the number of unprocessed groups.
                lastLeftGroupsOrderIdx--;
            }
        }
        // Convert password characters into a string and return the result.
        return new string(password);
    }
    /// <summary>
    /// Concatena un array di stringhe in una unica stringa risultatro.
    /// </summary>
    /// <param name="svValue">Array di stringhe</param>
    /// <returns>Stringa unificata</returns>
    public string Concat(string[] svValue)
    {
        string sTmpString = "";
        for (int i = 0; i < svValue.Length; i++)
        {
            sTmpString += svValue[i];
        }
        return sTmpString;
    }
    /// <summary>
    /// Serializza un DataTime nell'istanza dell'oggetto. La lunghezza della stringa è pari a 17 caratteri.
    /// </summary>
    /// <param name="oDate">Data da serializzare</param>
    public void SerializeDateTime(DateTime oDate)
    {
        string sAnno = oDate.Year.ToString().PadLeft(4, '0');
        string sMese = oDate.Month.ToString().PadLeft(2, '0');
        string sGiorno = oDate.Day.ToString().PadLeft(2, '0');
        string sOra = oDate.Hour.ToString().PadLeft(2, '0');
        string sMinuti = oDate.Minute.ToString().PadLeft(2, '0');
        string sSecondi = oDate.Second.ToString().PadLeft(2, '0');
        string sMillisecodi = oDate.Millisecond.ToString().PadLeft(3, '0');
        //
        _string = sAnno + sMese + sGiorno + sOra + sMinuti + sSecondi;
    }
    /// <summary>
    /// Serializza un Integer nell'istanza dell'oggetto. La lunghezza della stringa è pari a 4 caratteri.
    /// </summary>
    /// <param name="iValue">Data da serializzare</param>
    public void SerializeInt32(int iValue)
    {
        byte[] oBytes = BitConverter.GetBytes(iValue);
        string sValue = Encoding.Default.GetString(oBytes);
        _string = sValue;
    }
    /// <summary>
    /// Deserializza un DateTime dall'istanza dell'oggetto.
    /// </summary>
    public DateTime DeserializeDateTime()
    {
        try
        {
            DateTime tmp = DateTime.Parse(_string);
            return tmp;
        }
        catch
        {
            try
            {
                if (_string.Length == 8)
                {
                    cString[] svProva = SubStrings("4&2&2");
                    DateTime oDateTime = new DateTime(Convert.ToInt32(svProva[0]), Convert.ToInt32(svProva[1]), Convert.ToInt32(svProva[2]));
                    return oDateTime;
                }
                else
                {
                    throw new Exception();
                }
            }
            catch
            {
                throw new Exception();
            }
        }
    }
    /// <summary>
    /// Deserializza un Integer dall'istanza dell'oggetto.
    /// </summary>
    public int DeserializeInt32()
    {
        byte[] oBytes = Encoding.Default.GetBytes(_string);
        int iValue = BitConverter.ToInt32(oBytes, 0);
        return iValue;
    }
    /// <summary>
    /// Converte il contenuto dell'istanza in esadecimele.
    /// </summary>
    public string ToHex()
    {
        string hex = "";
        foreach (char c in _string)
        {
            int tmp = c;
            hex += string.Format("{0:x2}", Convert.ToUInt32(tmp.ToString()));
        }
        return hex;
    }
    /// <summary>
    /// Converte il contenuto dell'istanza da esadecimele.
    /// </summary>
    public string FromHex()
    {
        byte[] raw = new byte[_string.Length / 2];
        for (int i = 0; i < raw.Length; i++)
        {
            raw[i] = Convert.ToByte(_string.Substring(i * 2, 2), 16);
        }
        return Encoding.Default.GetString(raw);
    }
    /// <summary>
    /// Verifica se l'istanza contiene una stringa palindroma
    /// </summary>
    public bool IsPalindroma()
    {
        int sx, dx;
        bool trovato = true;
        for (sx = 0, dx = _string.Length - 1; sx < dx; sx++, dx--)
        {
            if (_string[sx] != _string[dx])
                trovato = false;
        }
        return trovato;
    }
    /// <summary>
    /// Controlla se tutti i catatteti dell'istanza sono lettere
    /// </summary>
    public bool IsAlfabetic()
    {
        if (!IsValidator(_string, vetChar))
            return false;
        return true;
    }
    /// <summary>
    /// Controlla se la data inserita nell'istanza è una data lavorativa.
    /// </summary>
    /// <returns>Ritona "True" se la data è lavorativa, "False" altrimenti.</returns>
    public bool IsDataLavorativa()
    {
        try
        {
            DateTime oDate = Convert.ToDateTime(_string);
            //
            // Sabato e Domenica
            if (oDate.DayOfWeek == DayOfWeek.Saturday)
                return false;
            if (oDate.DayOfWeek == DayOfWeek.Sunday)
                return false;
            //
            // Data Pasqua
            DateTime oDataPasqua;
            {
                int m = 24;
                int n = 5;
                int a = oDate.Year % 19;
                int b = oDate.Year % 4;
                int c = oDate.Year % 7;
                int d = (m + 19 * a) % 30;
                int e = (2 * b + 4 * c + 6 * d + n) % 7;
                int giorno = 22 + d + e;
                int mese = 3;
                if (giorno > 31)
                {
                    giorno = d + e - 9;
                    mese = 4;
                }
                oDataPasqua = new DateTime(oDate.Year, mese, giorno);
                if (oDate.ToShortDateString().Equals(oDataPasqua.ToShortDateString()))
                    return false;
            }
            //
            DateTime oDataPentecoste = new DateTime(oDataPasqua.Year, oDataPasqua.Month, oDataPasqua.Day);
            {
                oDataPentecoste = oDataPentecoste.AddDays(49);
                if (oDate.ToShortDateString().Equals(oDataPentecoste.ToShortDateString()))
                    return false;
            }
            //
            DateTime oDataLunediAngelo = new DateTime(oDataPasqua.Year, oDataPasqua.Month, oDataPasqua.Day);
            {
                oDataLunediAngelo = oDataLunediAngelo.AddDays(-48);
                if (oDate.ToShortDateString().Equals(oDataLunediAngelo.ToShortDateString()))
                    return false;
            }
            //
            DateTime oDataMercolediCeneri = new DateTime(oDataPasqua.Year, oDataPasqua.Month, oDataPasqua.Day);
            {
                oDataMercolediCeneri = oDataMercolediCeneri.AddDays(-46);
                if (oDate.ToShortDateString().Equals(oDataMercolediCeneri.ToShortDateString()))
                    return false;
            }
            //
            DateTime oDataVenerdiSanto = new DateTime(oDataPasqua.Year, oDataPasqua.Month, oDataPasqua.Day);
            {
                oDataVenerdiSanto = oDataVenerdiSanto.AddDays(-2);
                if (oDate.ToShortDateString().Equals(oDataVenerdiSanto.ToShortDateString()))
                    return false;
            }
            //
            DateTime oDataAscensione = new DateTime(oDataPasqua.Year, oDataPasqua.Month, oDataPasqua.Day);
            {
                oDataAscensione = oDataAscensione.AddDays(+39);
                if (oDate.ToShortDateString().Equals(oDataAscensione.ToShortDateString()))
                    return false;
            }
            //
            DateTime oDataCorpusDomini = new DateTime(oDataPasqua.Year, oDataPasqua.Month, oDataPasqua.Day);
            {
                oDataCorpusDomini = oDataCorpusDomini.AddDays(+60);
                if (oDate.ToShortDateString().Equals(oDataCorpusDomini.ToShortDateString()))
                    return false;
            }
        }
        catch
        {
            return false;
        }
        return true;
    }
    /// <summary>
    /// Controlla se l'istanza contiene un numero di carta di credito.
    /// </summary>
    public bool IsCreditCardNumber()
    {
        string cardNumber = _string.Replace(" ", "");

        //Primo Passo
        int[] doubledDigits = new int[cardNumber.Length / 2];
        int k = 0;
        for (int i = cardNumber.Length - 2; i >= 0; i -= 2)
        {
            int digit = int.Parse(cardNumber[i].ToString());
            doubledDigits[k] = digit * 2;
            k++;
        }

        //Secondo Passo: Add up separate digits
        int total = 0;
        foreach (int i in doubledDigits)
        {
            string number = i.ToString();
            for (int j = 0; j < number.Length; j++)
            {
                total += int.Parse(number[j].ToString());
            }
        }

        //Terzo Passo: Add up other digits
        int total2 = 0;
        for (int i = cardNumber.Length - 1; i >= 0; i -= 2)
        {
            int digit = int.Parse(cardNumber[i].ToString());
            total2 += digit;
        }

        //Quarto Passo: Totale
        int final = total + total2;

        return final % 10 == 0; //Well formed will divide evenly by 10
    }
    /// <summary>
    /// Controlla se l'istanza contiene un Datetime.
    /// </summary>
    public bool IsDateTime()
    {
        try
        {
            DateTime tmp = DateTime.Parse(_string);
            return true;
        }
        catch
        {
            return false;
        }
    }
    /// <summary>
    /// Controlla se ogni carattere nell'istanza contiene numeri.
    /// </summary>
    public bool IsNumeric()
    {
        if (!IsValidator(_string, vetNumeric))
            return false;
        return true;
    }
    /// <summary>
    /// Controlla se l'istanza contiene un Double.
    /// </summary>
    public bool IsDouble()
    {
        double tmp;
        try
        {
            tmp = double.Parse(_string);
            return true;
        }
        catch
        {
            return false;
        }
    }
    /// <summary>
    /// Controlla se l'istanza contiene un Double.
    /// </summary>
    public bool IsInteger()
    {
        int tmp;
        try
        {
            tmp = int.Parse(_string);
            return true;
        }
        catch
        {
            return false;
        }
    }
    /// <summary>
    /// Controlla se l'istanza contiene una Partita Iva.
    /// </summary>
    public bool IsPartitaIva()
    {
        if (_string.Trim().Length != 11)
            return false;
        if (!IsValidator(_string.Substring(0, 11), vetNumeric))
            return false;
        return true;
    }
    /// <summary>
    /// Controlla se l'istanza contiene una Codice Fiscale.
    /// </summary>
    public bool IsCodFis()
    {

        if (_string.Trim().Length != 16)
            return false;
        if (!IsValidator(_string.Substring(0, 6), vetChar))
            return false;
        if (!IsValidator(_string.Substring(6, 2), vetNumeric))
            return false;
        if (!IsValidator(_string.Substring(8, 1), vetChar))
            return false;
        if (!IsValidator(_string.Substring(9, 2), vetNumeric))
            return false;
        if (!IsValidator(_string.Substring(11, 1), vetChar))
            return false;
        if (!IsValidator(_string.Substring(12, 3), vetNumeric))
            return false;
        if (!IsValidator(_string.Substring(15, 1), vetChar))
            return false;
        return true;
    }
    /// <summary>
    /// Conta quante occorrenze di "carattere" sono presenti nell'istanza.
    /// </summary>
    public int Count(char carattere)
    {
        int conta = 0;

        for (int c2 = 0; c2 < _string.Length; c2++)
        {
            if (_string[c2] == carattere)
            {
                conta++;
            }
        }
        return conta;
    }
    /// <summary>
    /// Conta quante occorrenze di "stringa" sono presenti nell'istanza.
    /// </summary>
    public int Count(string stringa)
    {
        int posiz, cont = 0;
        posiz = _string.IndexOf(stringa);
        for (; posiz >= 0;)
        {
            cont++;
            posiz = _string.IndexOf(stringa, posiz + 1);
        }
        return cont;
    }
    /// <summary>
    /// Contolla se ogni catattere nell'istanza è presente in strCtrl
    /// </summary>
    /// <param name="strTarget">Stringa da controllare</param>
    /// <param name="strCtrl">Stringa di controllo</param>
    /// <returns></returns>
    public bool IsValidator(string strCtrl)
    {
        int p1, p2;
        bool bTrovato;
        for (p1 = 0; p1 < _string.Length; p1++)
        {
            bTrovato = false;
            for (p2 = 0; p2 < strCtrl.Length; p2++)
            {
                if (_string[p1] == strCtrl[p2])
                    bTrovato = true;
            }
            if (!bTrovato)
                return false;
        }
        return true;
    }
    /// <summary>
    /// Contolla se ogni catattere in strTarget è presente in strCtrl
    /// </summary>
    /// <param name="strTarget">Stringa da controllare</param>
    /// <param name="strCtrl">Stringa di controllo</param>
    /// <returns></returns>
    private bool IsValidator(string strTarget, string strCtrl)
    {
        int p1, p2;
        bool bTrovato;
        for (p1 = 0; p1 < strTarget.Length; p1++)
        {
            bTrovato = false;
            for (p2 = 0; p2 < strCtrl.Length; p2++)
            {
                if (strTarget[p1] == strCtrl[p2])
                    bTrovato = true;
            }
            if (!bTrovato)
                return false;
        }
        return true;
    }
    /// <summary>
    /// Controlla se ogni carattere nell'istanza contiene vocali.
    /// </summary>
    private bool IsVocale()
    {
        if (!IsValidator(_string, vetVocali))
            return false;
        return true;
    }
    /// <summary>
    /// Controlla se l'istanza è un'email.
    /// </summary>
    public bool IsEmail()
    {
        int posat, pospunt;
        if (_string.Trim().Length == 0)
            return false;
        posat = _string.IndexOf("@");
        if (posat <= 0)
            return false;
        pospunt = _string.IndexOf('.', posat);
        if (pospunt <= posat + 1)
            return false;
        if (_string.Trim().Length - 1 - pospunt <= 1)
            return false;
        return true;
    }
    /// <summary>
    /// Computa la stringa in ingresso nel seguente formato("4&8&13") e ritorna un array di stringhe con le lunghezze
    /// specificate in precedenza.
    /// </summary>
    /// <example>
    ///     <code>
    ///string oS = "abcdefghilmnopqrstuvz0123456789"
    ///oS.SubStrings("5&8");
    ///string[] oAS = oS.Split('&');
    ///
    ///Output:
    ///    oAS[0] abcde
    ///    oAS[1] fghilmno
    ///
    ///     </code>
    /// </example>
    public cString[] SubStrings(string sLunghezze)
    {
        try
        {
            string[] svLengh = sLunghezze.Split('&');
            cString[] svValues = new cString[svLengh.Length];
            cString sTemp = _string;
            int lenghtTemp = 0;
            int indexCurr = 0;
            int lenghtCurr = 0;
            for (int i = 0; i < svLengh.Length; i++)
            {
                lenghtTemp += Convert.ToInt32(svLengh[i]);
            }
            sTemp = sTemp.ToString().PadRight(lenghtTemp, ' ');
            for (int i = 0; i < svLengh.Length; i++)
            {
                lenghtCurr = Convert.ToInt32(svLengh[i]);
                svValues[i] = sTemp.ToString().Substring(indexCurr, lenghtCurr);
                indexCurr += lenghtCurr;
            }
            return svValues;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    /// <summary>
    /// Resititusce una matrice di oggetti String contenente le sottostringhe delimitatte dai tags specificati.
    /// </summary>
    /// <param name="sBeginTag">Tag di delimitazione iniziale.</param>
    /// <param name="sEndTag">Tag di delimitazione finale.</param>
    /// <returns></returns>
    public string[] SubStrings(string sBeginTag, string sEndTag)
    {
        cString oS = new cString(_string);
        string[] oV = oS.Split(sEndTag);
        List<string> oAL = new List<string>();
        for (int i = 0; i < oV.Length; i++)
        {
            if (oV[i].Trim().Equals(""))
                continue;
            if (oV[i].IndexOf(sBeginTag) < 0)
                continue;
            cString oS2 = new cString(oV[i]);
            string[] oV2 = oS2.Split(sBeginTag);
            oAL.Add(oV2[oV2.Length - 1]);
        }
        return oAL.ToArray();
    }
    public string ImportToText()
    {
        //
        if (IsDouble())
        {
            //
            string _string1 = _string;
            string[] str = _string1.Replace(",", ".").Split('.');
            //
            if (str.Length == 1)
                return SimpleValueToText(str[0]) + "/00";
            else
            {
                //
                if (str[1].Length == 1)
                    return SimpleValueToText(str[0]) + "/" + str[1] + "0";
                else
                    return SimpleValueToText(str[0]) + "/" + str[1];
                //
            }
            //
            //				return SimpleValueToText(_string);
        }
        return "";
        //
    }
    /// <summary>
    /// Converte l'istanza contenente un'intero scritto in una stringa e ritorna una stringa contenente il nome dell'intero.
    /// <example>
    ///     <code>
    ///cString oS = "4";
    ///
    ///Output:
    ///
    ///     oS.ValueToText() -> quattro;
    ///     </code>
    /// </example>
    /// </summary>
    public string ValueToText()
    {
        return SimpleValueToText(_string);
    }

    private string SimpleValueToText(string sNumberValue)
    {
        string[] vetUNO = new string[] { "zero", "uno", "due", "tre", "quattro", "cinque", "sei", "sette", "otto", "nove" };
        string[] vetDIECI = new string[] { "dieci", "undici", "dodici", "tredici", "quattordici", "quindici", "sedici", "diciasette", "diciotto", "diciannove" };
        string[] vetVENTI = new string[] { "venti", "trenta", "quaranta", "cinquanta", "sessanta", "settanta", "ottanta", "novanta" };
        string[] vetMILLE = new string[] { "mille", "duemila", "tremila", "quattromila", "cinquemila", "seimila", "settemila", "ottomila", "novemila" };
        int intNumberValue = int.Parse(sNumberValue.Trim());
        string strNumberValue = intNumberValue.ToString();
        //
        if (strNumberValue.Length == 7)
            if (strNumberValue[0].ToString().Equals("1"))
                return "unmilione" + SimpleValueToText(strNumberValue.Substring(1));
            else
                return vetUNO[int.Parse(strNumberValue[0].ToString())] + "milioni" + SimpleValueToText(strNumberValue.Substring(1));
        //
        if (strNumberValue.Length == 6)
            return SimpleValueToText(strNumberValue.Substring(0, 3)) + "mila" + SimpleValueToText(strNumberValue.Substring(3)).Replace("zero", "");
        //
        if (strNumberValue.Length == 5)
            return SimpleValueToText(strNumberValue.Substring(0, 2)) + "mila" + SimpleValueToText(strNumberValue.Substring(2)).Replace("zero", "");
        //
        if (strNumberValue.Length == 4)
            if (strNumberValue[0].ToString().Equals("1"))
                return "mille" + SimpleValueToText(strNumberValue.Substring(1));
            else
                return vetUNO[int.Parse(strNumberValue[0].ToString())] + "mila" + SimpleValueToText(strNumberValue.Substring(1));
        //
        if (strNumberValue.Length == 3)
        {
            if (strNumberValue[0].ToString().Equals("1"))
                return "cento" + SimpleValueToText(strNumberValue.Substring(1));
            else
                return vetUNO[int.Parse(strNumberValue[0].ToString())] + "cento" + SimpleValueToText(strNumberValue.Substring(1));
        }
        //
        if (strNumberValue.Length == 2)
        {
            if (intNumberValue < 20)
                return vetDIECI[intNumberValue - 10];
            else
                return MargeNumberText(vetVENTI[int.Parse(strNumberValue[0].ToString()) - 2], vetUNO[int.Parse(strNumberValue[1].ToString())]);
        }
        //
        if (strNumberValue.Length == 1)
        {
            if (intNumberValue != 0)
                return vetUNO[intNumberValue];
        }
        //
        return "";
    }

    //
    // Prima: <div shgkshd><p>skjdhfkjsd</p></div>
    // Dopo:  <div shgkshd></div>
    private string DelTag(string sString, string sTagBegin, string sTagEnd, ref int index)
    {
        int iBBody = sString.IndexOf(sTagBegin, index);
        if (iBBody != -1)
        {
            int iEBody = sString.IndexOf(sTagEnd, iBBody + sTagBegin.Length + 1);
            if (iEBody != -1)
            {
                index = iBBody;//iEBody + ((string)(sTagEnd)).Length;
                return string.Concat(sString.Substring(0, iBBody), sString.Substring(iEBody + sTagEnd.Length));
            }
        }
        index = -1;
        return sString;
    }
    /// <summary>
    /// Identifica le sottostringhe contenute tra sTagBegin e sTagEnd eliminandole, saranno eliminate anche le stesse stringhe sTagBegin e sTagEnd.
    /// </summary>
    /// <param name="sTagBegin">Tag di partenza.</param>
    /// <param name="sTagEnd">Tag di arrivo.</param>
    public string DelTag(string sTagBegin, string sTagEnd)
    {
        string sString = _string;
        int index = 0;
        for (; index != -1;)
        {
            sString = DelTag(sString, sTagBegin, sTagEnd, ref index);
        }
        return sString;
    }
    /// <summary>
    /// Ritorna le sottostringhe contenute tra sTagBegin e sTagEnd.
    /// </summary>
    /// <param name="sTagBegin">Tag di partenza.</param>
    /// <param name="sTagEnd">Tag di arrivo.</param>
    /// <returns></returns>
    public string[] GetIntoTag(string sTagBegin, string sTagEnd)
    {
        List<string> vString = new List<string>();
        int index = 0;
        for (; index != -1;)
        {
            string sValue = GetIntoTag(_string, sTagBegin, sTagEnd, ref index);
            if (sValue != null && index != -1)
                vString.Add(sValue);
        }
        return vString.ToArray();
    }



    private string GetIntoTag(string sString, string sTagBegin, string sTagEnd, ref int index)
    {
        int iBBody = sString.IndexOf(sTagBegin, index);
        if (iBBody != -1)
        {
            int iEBody = sString.IndexOf(sTagEnd, iBBody + sTagBegin.Length + 1);
            if (iEBody != -1)
            {
                index = iEBody + 1;
                return sString.Substring(iBBody + sTagBegin.Length, iEBody - (iBBody + sTagBegin.Length));
            }
        }
        index = -1;
        return sString;
    }

    private string MargeNumberText(string sNumberValue1, string sNumberValue2)
    {
        if (sNumberValue2.Equals("zero"))
            return sNumberValue1;
        cString oS1 = sNumberValue1[sNumberValue1.Length - 1].ToString();
        cString oS2 = sNumberValue2[0].ToString();
        if (oS1.IsVocale() && oS2.IsVocale())
            return sNumberValue1.Substring(0, sNumberValue1.Length - 1) + sNumberValue2;
        return sNumberValue1 + sNumberValue2;
    }
    /// <summary>
    /// Consente di determinare se l'oggeto specificato ha lo stesso valore dell'istanza. 
    /// </summary>
    /// <param name="str">Stringa da confrontare.</param>
    public bool Equals(string str)
    {
        return _string.Equals(str);
    }
    // string to SuperString
    // DBBool.dbTrue and false to DBBool.dbFalse:
    public static implicit operator cString(string x)
    {
        cString s = new cString(x);
        return s;
    }
    /// <summary>
    /// Conversione implicita tra cString e String.
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    public static implicit operator string(cString x)
    {
        return x._string;
    }
    //
    /// <summary>
    /// Override del metodo ToString().
    /// </summary>
    public override string ToString()
    {
        return _string;
    }
}
