﻿namespace ComPlugin
{
    using ComCalculatorLib;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    
    [ComVisible(true)]
    [ProgId("ComPlugin.MyCalculator")]
    [Guid("2C1B9DAA-E436-43C2-9F77-3C48BA2F8537")]
    public class MyCalculator : ITheCalculator
    {
        CalculatorApplication application;
        SecondaryApplicationDomainManager secondaryApplicationDomain;

        public void Initialize(CalculatorApplication application)
        {
            Helper.ReportDomain("MyCalculator");
            this.application = application;
            this.secondaryApplicationDomain = new SecondaryApplicationDomainManager();
        }

        private void InitializeSecondaryApplicationDomain()
        {
        }

        public int Add(int left, int right)
        {
            return left + right * 10;
        }
    }

    public class SecondaryApplicationDomainManager : MarshalByRefObject
    {
        AppDomain applicationDomain;
        InDomainObject inDomainObject;

        public SecondaryApplicationDomainManager()
        {
            AppDomainSetup setup = new AppDomainSetup()
            {
                ApplicationBase = Path.GetDirectoryName(typeof(SecondaryApplicationDomainManager).Assembly.Location)
            };

            applicationDomain = AppDomain.CreateDomain("Secondary Domain", null, setup);

            var typeToCreateInSecondaryDomain = typeof(InDomainObject);
            var assemblyOfTypeToCreateInSecondaryDomain = typeToCreateInSecondaryDomain.Assembly;
            var nameOfAssembly = assemblyOfTypeToCreateInSecondaryDomain.FullName;

            AppDomain.CurrentDomain.AssemblyResolve += (sender, e) =>
            {
                var name = new AssemblyName(e.Name);

                if (name.FullName == nameOfAssembly)
                {
                    return assemblyOfTypeToCreateInSecondaryDomain;
                }

                return null; 
            };

            var inDomainObjectTemp = applicationDomain.CreateInstanceAndUnwrap(nameOfAssembly, typeToCreateInSecondaryDomain.FullName);
            inDomainObject = (InDomainObject)inDomainObjectTemp;
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }
    }

    public class InDomainObject : MarshalByRefObject
    {
        public InDomainObject()
        {
            Helper.ReportDomain("InDomainObject");
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }
    }

    internal static class Helper
    {
        public static void ReportDomain(string who)
        {
            Console.WriteLine("{0} loaded into: '{1}', CLR version '{2}'.", who, AppDomain.CurrentDomain.FriendlyName, Environment.Version);
        }
    }
}
