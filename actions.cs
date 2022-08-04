/*using System;
using Microsoft.TemplateEngine.Abstractions;
using Microsoft.TemplateEngine.Cli.PostActionProcessors;

namespace PostActions
{
    public class TestAction : IPostActionProcessor
    {
        public static readonly Guid ActionProcessorId = new Guid("01483af4-3f00-454c-9412-69ea39a62f39");

        public Guid Id => ActionProcessorId;

        public bool Process(IEngineEnvironmentSettings settings, IPostAction actionConfig, ICreationResult templateCreationResult, string outputBasePath)
        {
            settings.Host.LogMessage("HI MOMMMMM");
            Console.WriteLine("IS THIS WORKING");

            throw new Exception("foooo");

            // return true;
        }
    }
}*/
