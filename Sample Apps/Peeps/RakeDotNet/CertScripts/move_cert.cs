using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;

namespace MoveCert
{
    class Program
    {
        static void Main(string[] args)
        {
            var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            var root = new X509Store(StoreName.Root, StoreLocation.LocalMachine);

            store.Open(OpenFlags.ReadWrite);
            root.Open(OpenFlags.ReadWrite);

            dynamic myCert = null;

            var certificates = store.Certificates;
            foreach (var certificate in certificates)
            {
                if(certificate.IssuerName.Name == "CN=" + args[0])
                {
                    myCert = certificate;
                    break;
                }
            }

            store.Remove(myCert);
            root.Add(myCert);
            store.Close();
            root.Close();
        }
    }
}

