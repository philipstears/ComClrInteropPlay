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

            this.secondaryApplicationDomain.InDomainObject.SmuggleApplication(
                    Marshal.GetIUnknownForObject(application)
            );

            this.secondaryApplicationDomain.InDomainObject.WreakHavoc();
        }

        public int Add(int left, int right)
        {
            return left + right;
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

        public InDomainObject InDomainObject
        {
            get { return inDomainObject; }
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }
    }

    public class InDomainObject : MarshalByRefObject
    {
        private CalculatorApplication mApplication;

        public InDomainObject()
        {
            Helper.ReportDomain("InDomainObject");
        }

        public void SmuggleApplication(IntPtr punkApplication)
        {
            mApplication = (CalculatorApplication)Marshal.GetTypedObjectForIUnknown(punkApplication, typeof(CalculatorApplication));
        }

        public void WreakHavoc()
        {
            mApplication.LoadPlugin("ComPlugin.OtherPlugin");
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
