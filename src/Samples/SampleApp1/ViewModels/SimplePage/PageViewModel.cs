using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;
using SampleApp1.Models;

namespace SampleApp1.ViewModels.SimplePage
{
    public class PageViewModel : DotvvmViewModelBase
    {
        public AddressType AddressType { get; set; }

        public string Name { get; set; }

        public string Address { get; set; }

        public bool IsEuVatPayer { get; set; }

        public List<string> Countries { get; set; } = Data.Countries;

        public string CountryCode { get; set; }

        public string StatusMessage { get; set; }

        public void CreateCompany()
        {
            StatusMessage = $"The company {Name} was created.";
        }

        public void ResetForm()
        {
            AddressType = AddressType.Person;
            Name = "";
            Address = "";
            IsEuVatPayer = false;
            CountryCode = "";
            StatusMessage = "";
        }

    }
}

