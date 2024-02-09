namespace Solution.Infrastructure.Models;
public class esito
{
    public esito()
    {
        code = "200";
        description = "OK";
    }
    public string code { get; set; }
    public string description { get; set; }
}

public class JSONResponse<T>
{
    public JSONResponse()
    {
        this.errorStatus = new esito();
    }

    public JSONResponse(string sCode, string sMessage, T oResponse)
    {
        this.errorStatus = new esito();
        this.errorStatus.code = sCode;
        this.errorStatus.description = sMessage;
        //
        response = oResponse;
    }
    public esito errorStatus { get; set; }
    public T response { get; set; }
}