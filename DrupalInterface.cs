using System.Collections;
using System.Net;
using System.IO;
using System.Collections.Specialized;

/// <summary>
/// 6/1/2016
/// Charles Palen - www.transcendingdigital.com
/// 
/// This class was created in order to communicate with Drupal 8 REST
/// services. It was developed against version 8.1.1
/// 
/// This class uses BASIC AUTH. If you are using this on the Internet
/// you should make sure all communication is https.
/// Basic auth includes base64 encoded username and password with each
/// request.
/// 
/// This class also focuses on HAL JSON.
/// 
/// In Drupal 8, you can use Basic Auth with no CSRF token when using GET
/// 
/// When using POST we have to also utilize CSRF tokens
/// 
/// Should be able to be used in C# applications; but targeted for use in Unity3D.
/// 
/// THIS CLASS BLOCKS FOR AN UNDEFINED AMOUNT OF TIME WHILE PREFORMING THE WEB REQUESTS
/// </summary>
public class DrupalInterface {

    private const string _HAL_Format_Query_Param = "_format=hal_json";
    private const string _HAL_Accept_Header = "application/hal+json";

    private const string METHOD_NODE_RETRIEVE = "/node/";
    private const string METHOD_CSRF_TOKEN = "/rest/session/token";

    /// <summary>
    /// Should include the whole protocol..like https://127.0.0.1
    /// </summary>
    private string _serverURL = "";
    private string _userName = "";
    private string _password = "";

    private string _genratedBasicAuthHeader = "";
    private string _mostRecentCSRFToken = "";

    public DrupalInterface(string serverURL, string username, string password)
    {
        _serverURL = serverURL;
        _userName = username;
        _password = password;

        _genratedBasicAuthHeader = genBasicAuthHeader();
    }
    /// <summary>
    /// Attempts to get a node with the specified id.
    /// Returns in HAL JSON format.
    /// </summary>
    /// <param name="_nodeID"></param>
    public void getNode(int _nodeID)
    {
        HttpWebRequest thisHttpRequest = (HttpWebRequest)WebRequest.Create(_serverURL + METHOD_NODE_RETRIEVE + _nodeID.ToString() + "?" + _HAL_Format_Query_Param);
        // Cookies are disabled by default..good.
        thisHttpRequest.Method = "GET";

        // Add/Edit headers
        thisHttpRequest.Accept = _HAL_Accept_Header;
        thisHttpRequest.Headers["Authorization"] = _genratedBasicAuthHeader;

        // Send to the consolodated response handler
        executeWebRequest(ref thisHttpRequest, METHOD_NODE_RETRIEVE);
    }
    public void doPostRequest()
    {
        getCSRFToken();
        // TO DO - implement an actual post request

    }
    /// <summary>
    /// For basic auth you create a total string in the format
    /// Basic base64Payload
    /// 
    /// base64Payload is the username a color and the password base64 encoded.
    /// Explained in another way not base64 encoded the auth header looks like this
    /// Basic username:password
    /// 
    /// </summary>
    /// <returns></returns>
    private string genBasicAuthHeader()
    {
        string returnString = "Basic ";

        string nonB64PayloadString = _userName + ":" + _password;
        string finalB64Payload = "";
        try
        {
            byte[] plainTxtBytes = System.Text.Encoding.UTF8.GetBytes(nonB64PayloadString);
            finalB64Payload = System.Convert.ToBase64String(plainTxtBytes, 0, plainTxtBytes.Length);
        }
        catch (System.ArgumentNullException)
        {
            System.Console.WriteLine("DrupalInterface - genBasicAuthHeader - error creating basic auth header using b64");
            
        }

        returnString = returnString + finalB64Payload;

        return returnString;
    }
    /// <summary>
    /// We have to get a csrf token by
    /// sending a GET request to:
    /// /rest/session/token
    /// 
    /// With no headers..nothing
    /// </summary>
    private void getCSRFToken()
    {
        HttpWebRequest thisHttpRequest = (HttpWebRequest)WebRequest.Create(_serverURL + METHOD_CSRF_TOKEN);
        // Cookies are disabled by default..good.
        thisHttpRequest.Method = "GET";

        // Send to the consolodated response handler
        executeWebRequest(ref thisHttpRequest, METHOD_CSRF_TOKEN);
    }
    /// <summary>
    /// This is where the code will block execution.
    /// </summary>
    /// <param name="_theRequestRef"></param>
    /// <param name="_responseType"></param>
    private void executeWebRequest(ref HttpWebRequest _theRequestRef, string _responseType)
    {
        WebResponse wresp = null;
        try
        {
            wresp = _theRequestRef.GetResponse();
            Stream responseStream = wresp.GetResponseStream();
            StreamReader responseReader = new StreamReader(responseStream);
            string jsonRawResponse = responseReader.ReadToEnd();

            switch (_responseType)
            {
                case METHOD_NODE_RETRIEVE:
                    System.Console.WriteLine(string.Format("Node retrieved, server response is: {0}", jsonRawResponse));
                    break;
                case METHOD_CSRF_TOKEN:
                    System.Console.WriteLine(string.Format("CSRF Token retrieved, server response is: {0}", jsonRawResponse));
                    break;
            }

            // Send the response off for parsing
            parseDrupalJSONResponse(jsonRawResponse, _responseType);
        }
        catch (WebException ex)
        {
            using (WebResponse response = ex.Response)
            {
                HttpWebResponse httpResponse = (HttpWebResponse)response;

                switch (_responseType)
                {
                    case METHOD_NODE_RETRIEVE:
                        System.Console.WriteLine("Error recieving node: {0} Description: {1}", httpResponse.StatusCode, httpResponse.StatusDescription);
                        break;
                    case METHOD_CSRF_TOKEN:
                        System.Console.WriteLine("Error recieving CSRF Token: {0} Description: {1}", httpResponse.StatusCode, httpResponse.StatusDescription);
                        break;
                }

                using (Stream data = response.GetResponseStream())
                {
                    string text = new StreamReader(data).ReadToEnd();
                    System.Console.WriteLine(text);
                    data.Close();
                }
                httpResponse.Close();
            }

            if (wresp != null)
            {
                wresp.Close();
                wresp = null;
            }
        }
        finally
        {
            _theRequestRef = null;
        }

    }
    /// <summary>
    /// Parses raw json from Drupal to figure out what to do with it.
    /// 
    /// </summary>
    /// <param name="_rawJson"></param>
    /// <param name="_responseType"></param>
    private void parseDrupalJSONResponse(string _rawJson, string _responseType)
    {
        switch (_responseType)
        {
            case METHOD_NODE_RETRIEVE:
                //System.Console.WriteLine(string.Format("Node retrieved, server response is: {0}", jsonRawResponse));
                // Up to you to parse the response here

                break;
            case METHOD_CSRF_TOKEN:
               // System.Console.WriteLine(string.Format("CSRF Token retrieved, server response is: {0}", jsonRawResponse));
               if(_rawJson.Length > 0)
                {
                    _mostRecentCSRFToken = _rawJson;
                }
                break;
        }
    }
}
