namespace Solution.Data;
/// <summary>
/// Classe contenente la gestione di documenti XML.
/// </summary>
public class cXMLManager
{
    protected XmlDocument _registry = null;
    protected string _version = "1.0";
    protected string _strKey = "";
    protected string _strFileName = "";
    private cCollection _oReplaces = null;
    public event EventHandler OnChange;

    #region Constructors
    /// <summary>
    /// Inizializza l'istanza.
    /// </summary>
    public cXMLManager()
    {
        _registry = new XmlDocument();
        _registry.LoadXml(CreateXML());
    }

    public cXMLManager(string sFileName)
    {
        Load(sFileName);
    }
    /// <summary>
    /// File a cui si fa riferimento.
    /// </summary>
    public string Filename
    {
        get
        {
            return _strFileName;
        }
    }
    /// <summary>
    /// Contenuto xml dell'oggetto.
    /// </summary>
    public string InnerXml
    {
        set { _registry.InnerXml = value; }
        get { return _registry.InnerXml; }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sStringXML"></param>
    public void LoadXml(string sStringXML)
    {
        _strFileName = "";
        _registry = new XmlDocument();
        _registry.LoadXml(sStringXML);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="oReplaces"></param>
    public void SetReplace(cCollection oReplaces)
    {
        _oReplaces = oReplaces;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sValue"></param>
    /// <returns></returns>
    private string Replace(string sValue)
    {
        if (_oReplaces == null || _oReplaces.Count == 0)
            return sValue;
        for (int i = 0; i < _oReplaces.Count; i++)
        {
            sValue = sValue.Replace((string)_oReplaces.GetKey(i), (string)_oReplaces.GetValue(i));
        }
        return sValue;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sFileName"></param>
    public void Load(string sFileName)
    {
        try
        {
            _strFileName = sFileName;
            _registry = new XmlDocument();
            if (!File.Exists(sFileName))
                _registry.LoadXml(CreateXML());
            else
                _registry.Load(sFileName);
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public void Load(cXMLManager oXML)
    {
        _registry = (XmlDocument)oXML._registry.CloneNode(true);
        _strFileName = oXML.Filename;
    }
    #endregion Constructors
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sParams"></param>
    /// <returns></returns>
    private cCollection GetAttributeFromParams(string sParams)
    {
        cCollection oAttribute = new cCollection();
        //
        string[] sValues = sParams.Split('&');
        for (int i = 0; i < sValues.Length; i++)
        {
            string[] sAttribute = sValues[i].Split('=');
            if (sAttribute.Length == 2)
            {
                oAttribute.Add(sAttribute[0], sAttribute[1]);
            }
        }
        return oAttribute;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sParams"></param>
    /// <returns></returns>
    private string GetElementFromParams(string sParams)
    {
        cCollection oAttribute = new cCollection();
        //
        string[] sValues = sParams.Split('&');
        for (int i = 0; i < sValues.Length; i++)
        {
            string[] sAttribute = sValues[i].Split('=');
            if (sAttribute.Length == 1)
            {
                return sAttribute[0];
            }
        }
        return "";
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="oNode"></param>
    /// <param name="oAttributes"></param>
    /// <returns></returns>
    private bool CompareAttribute(XmlNode oNode, cCollection oAttributes)
    {
        for (int i = 0; i < oNode.Attributes.Count; i++)
        {
            if (oAttributes.ContainsKey(oNode.Attributes[i].Name))
            {
                if (!oAttributes[oNode.Attributes[i].Name].Equals(oNode.Attributes[i].Value))
                    return false;
            }
        }
        return true;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="oNode"></param>
    /// <param name="sValue"></param>
    /// <returns></returns>
    private bool Compare(XmlNode oNode, string sValue)
    {
        string sElement = GetElementFromParams(sValue);
        cCollection oAttribute = GetAttributeFromParams(sValue);
        //
        if (oNode.NodeType == XmlNodeType.Comment)
            return false;
        if (String.Compare(oNode.Name.ToLower(), sElement.ToLower()) == 0)
        {
            if (CompareAttribute(oNode, oAttribute))
                return true;
        }
        return false;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sPath"></param>
    /// <returns></returns>
    private XmlNode GetElement(params string[] sPath)
    {
        XmlNode tmpNode = null;
        if (sPath == null || sPath.Length == 0 || _registry == null || _registry.ChildNodes == null)
            return null;
        //
        for (int i = 0; i < _registry.ChildNodes.Count; i++)
        {
            if (Compare(_registry.ChildNodes[i], sPath[0]))
            {
                tmpNode = _registry.ChildNodes[i];
            }
        }
        //
        for (int j = 1; j < sPath.Length; j++)
        {
            bool bTrovato = false;
            for (int i = 0; i < tmpNode.ChildNodes.Count && !bTrovato; i++)
            {
                if (Compare(tmpNode.ChildNodes[i], sPath[j]))
                {
                    bTrovato = true;
                    tmpNode = tmpNode.ChildNodes.Item(i);
                }
            }
            if (!bTrovato)
            {
                return null;
            }
        }
        return tmpNode;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="xmlNode"></param>
    /// <returns></returns>
    private cCollection GetAttributes(XmlNode xmlNode)
    {
        cCollection oAttributers = new cCollection();
        try
        {
            if (xmlNode != null)
            {
                for (int i = 0; i < xmlNode.Attributes.Count; i++)
                {
                    oAttributers.Add(xmlNode.Attributes[i].Name, xmlNode.Attributes[i].Value);
                }
            }
            return oAttributers;
        }
        catch (Exception e)
        {
            throw e;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="xmlNode"></param>
    /// <param name="oAttributers"></param>
    private void SetAttributes(XmlNode xmlNode, cCollection oAttributers)
    {
        try
        {
            if (xmlNode != null)
            {
                //
                for (int i = 0; i < oAttributers.Count; i++)
                {
                    if (xmlNode.Attributes.GetNamedItem((string)oAttributers.GetKey(i)) != null)
                    {
                        xmlNode.Attributes[(string)oAttributers.GetKey(i)].Value = (string)oAttributers.GetValue(i);
                    }
                    else
                    {
                        XmlAttribute oX = _registry.CreateAttribute((string)oAttributers.GetKey(i));
                        oX.Value = (string)oAttributers.GetValue(i);
                        xmlNode.Attributes.Append(oX);
                    }
                }
            }
        }
        catch (Exception e)
        {
            throw e;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sPath"></param>
    /// <param name="sDefaultValue"></param>
    /// <returns></returns>
    public string Get(string sPath, string sDefaultValue)
    {
        string[] svPath = sPath.Split('/');
        XmlNode oX = GetElement(svPath);
        if (oX != null)
            return Replace(oX.InnerText);
        else
            return Replace(sDefaultValue);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sPath"></param>
    /// <param name="sAttributeName"></param>
    /// <param name="sDefaultValue"></param>
    /// <returns></returns>
    public string Get(string sPath, string sAttributeName, string sDefaultValue)
    {
        string[] svPath = sPath.Split('/');
        XmlNode oX = GetElement(svPath);
        if (oX != null)
        {
            if (oX.Attributes.GetNamedItem(sAttributeName) != null)
            {
                return Replace(oX.Attributes[sAttributeName].Value);
            }
            else
            {
                return Replace(sDefaultValue);
            }
        }
        else
        {
            return Replace(sDefaultValue);
        }
    }

#if (!MOBILE)
    /// <summary>
    /// Metodo per invocare query Xpath.
    /// </summary>
    /// <param name="sExpression">Espressione XPath</param>
    /// <returns>Array di valori</returns>
    public string[] GetX(string sExpression)
    {
        XPathNavigator oXPN = _registry.CreateNavigator();
        XPathExpression expr = oXPN.Compile(sExpression);
        XPathNodeIterator iterator = oXPN.Select(expr);
        //
        ArrayList oA = new ArrayList();
        while (iterator.MoveNext())
        {
            XPathNavigator nav2 = iterator.Current.Clone();
            oA.Add(Replace(nav2.Value));
        }
        if (oA.Count <= 0)
            return null;
        return (string[])oA.ToArray(typeof(string));
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sExpression"></param>
    /// <param name="sDefault"></param>
    /// <returns></returns>
    public string GetX(string sExpression, string sDefault)
    {
        string[] oStrings = GetX(sExpression);
        if (oStrings == null)
            return Replace(sDefault);
        else
            return oStrings[0];
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sExpression"></param>
    /// <param name="sValue"></param>
    /// <returns></returns>
    public bool SetX(string sExpression, string sValue)
    {
        XmlNodeList oXNL = null;
        XmlElement root = _registry.DocumentElement;
        oXNL = root.SelectNodes(sExpression);
        if (oXNL != null)
        {
            for (int i = 0; i < oXNL.Count; i++)
            {
                if (oXNL[i].NodeType == XmlNodeType.Attribute)
                {
                    oXNL[i].Value = sValue;
                }
                else
                {
                    oXNL[i].InnerXml = sValue;
                }
            }
        }
        return true;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sExpression"></param>
    /// <param name="sValue"></param>
    /// <returns></returns>
    public bool InsertX(string sExpression, string sValue)
    {
        XmlNodeList oXNL = null;
        XmlElement root = _registry.DocumentElement;
        oXNL = root.SelectNodes(sExpression);
        if (oXNL != null)
        {
            for (int i = 0; i < oXNL.Count; i++)
            {
                if (oXNL[i].NodeType == XmlNodeType.Attribute)
                {
                    oXNL[i].Value = sValue;
                }
                else
                {
                    try
                    {
                        XmlDocumentFragment docFrag = _registry.CreateDocumentFragment();
                        docFrag.InnerXml = sValue;
                        oXNL[i].InsertAfter(docFrag, oXNL[i].LastChild);
                    }
                    catch
                    {
                        XmlDocumentFragment docFrag = _registry.CreateDocumentFragment();
                        docFrag.InnerText = sValue;
                        oXNL[i].InsertAfter(docFrag, oXNL[i].LastChild);
                    }
                }
            }
        }
        return true;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sExpression"></param>
    /// <param name="sAttribute"></param>
    /// <param name="sValue"></param>
    /// <returns></returns>
    public bool InsertX(string sExpression, string sAttribute, string sValue)
    {
        XmlNodeList oXNL = null;
        XmlElement root = _registry.DocumentElement;
        oXNL = root.SelectNodes(sExpression);
        if (oXNL != null)
        {
            for (int i = 0; i < oXNL.Count; i++)
            {
                if (oXNL[i].NodeType == XmlNodeType.Attribute)
                {
                    oXNL[i].Value = sValue;
                }
                else
                {
                    if (oXNL[i].Attributes.GetNamedItem(sAttribute) == null)
                    {
                        XmlAttribute docA = _registry.CreateAttribute(sAttribute);
                        docA.Value = sValue;
                        oXNL[i].Attributes.Append(docA);
                    }
                    else
                    {
                        oXNL[i].Attributes.GetNamedItem(sAttribute).Value = sValue;
                    }
                }
            }
        }
        return true;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sExpression"></param>
    /// <returns></returns>
    public bool DelX(string sExpression)
    {
        XmlNodeList oXNL = null;
        XmlElement root = _registry.DocumentElement;
        oXNL = root.SelectNodes(sExpression);
        if (oXNL != null)
        {
            for (int i = 0; i < oXNL.Count; i++)
            {
                if (oXNL[i].NodeType == XmlNodeType.Attribute)
                {
                    XmlElement XMLN = ((XmlAttribute)oXNL[i]).OwnerElement;
                    XMLN.Attributes.Remove(((XmlAttribute)oXNL[i]));
                }
                else
                {
                    XmlNode XMLN = oXNL[i].ParentNode;
                    XMLN.RemoveChild(oXNL[i]);
                }
            }
        }
        return true;
    }
#endif
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sPath"></param>
    /// <param name="sValue"></param>
    public void Set(string sPath, string sValue)
    {
        string[] svPath = sPath.Split('/');
        XmlNode oX = SetElement(svPath);
        if (oX != null && sValue != null)
            oX.InnerText = sValue;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sPath"></param>
    /// <param name="sAttributeName"></param>
    /// <param name="sAttributeValue"></param>
    public void Set(string sPath, string sAttributeName, string sAttributeValue)
    {
        string[] svPath = sPath.Split('/');
        XmlNode oX = SetElement(svPath);
        if (oX != null)
        {
            if (oX.Attributes.GetNamedItem(sAttributeName) != null)
            {
                oX.Attributes[sAttributeName].Value = sAttributeValue;
            }
            else
            {
                XmlAttribute oXAtt = _registry.CreateAttribute(sAttributeName);
                oXAtt.Value = sAttributeValue;
                oX.Attributes.Append(oXAtt);
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sPath"></param>
    /// <returns></returns>
    private XmlNode SetElement(params string[] sPath)
    {
        XmlNode tmpNode = null;
        if (sPath == null || sPath.Length == 0 || _registry == null || _registry.ChildNodes == null)
            return null;
        //
        for (int i = 0; i < _registry.ChildNodes.Count; i++)
        {
            if (Compare(_registry.ChildNodes[i], sPath[0]))
            {
                tmpNode = _registry.ChildNodes[i];
            }
        }
        //
        try
        {
            for (int i = 1; i < sPath.Length; i++)
            {
                XmlNode tmpNodeChild = null;
                for (int j = 0; j < tmpNode.ChildNodes.Count; j++)
                {
                    if (Compare(tmpNode.ChildNodes[j], sPath[i]))
                    {
                        tmpNodeChild = tmpNode.ChildNodes.Item(j);
                    }
                }
                if (tmpNodeChild == null)
                {
                    tmpNodeChild = _registry.CreateNode(XmlNodeType.Element, GetElementFromParams(sPath[i]), "");
                    tmpNode.AppendChild(tmpNodeChild);
                }
                tmpNode = tmpNodeChild;
                SetAttributes(tmpNode, GetAttributeFromParams(sPath[i]));
            }
            if (OnChange != null)
                OnChange(this, new EventArgs());
        }
        catch (Exception e)
        {
            throw e;
        }
        return tmpNode;
    }
    /// <summary>
    /// 
    /// </summary>
    public void Save()
    {
        Save(_strFileName);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sFileName"></param>
    public void Save(string sFileName)
    {
        try
        {
            _registry.Save(sFileName);
        }
        catch (Exception e)
        {
            throw e;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private string CreateXML()
    {
        string myStr = "";
        try
        {
            myStr = @"<?xml version=""1.0""?>";
            myStr += @"<registry name=""XMLManager"" version="""">";
            myStr += @"</registry>";
            return myStr;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        if (_registry != null)
            return Replace(_registry.InnerXml);
        return "";
    }
}
