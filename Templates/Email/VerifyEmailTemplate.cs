using Scriban;
using Scriban.Runtime;
using RetailAppS.Templates;
using RetailAppS.Lang;

namespace RetailAppS.Templates.Email
{
    public class VerifyEmailTemplate
    {
        public static string CLASS_NAME = typeof(VerifyEmailTemplate).Name;

        public static string GenerateVerifyEmailTemplate(TemplateService templateSerivice, string verifyEmailUrl)
        {
            var scriptObject1 = new ScriptObject();
            scriptObject1.Add("verifyEmailUrl", verifyEmailUrl);

            var context = new TemplateContext();
            context.PushGlobal(scriptObject1);

            var template = templateSerivice.ParseFile("Email/VerifyEmailTemplate", true);

            var result = template.Render(context);


            return result;
        }
    }
}
