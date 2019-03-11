﻿using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Tools.SeleniumGenerator.Generators.Controls
{
    public class FileUploadControlGenerator : SeleniumGenerator<FileUpload>
    {
        private static readonly DotvvmProperty[] nameProperties = { FileUpload.UploadCompletedProperty, FileUpload.UploadButtonTextProperty, FileUpload.UploadedFilesProperty };

        public override DotvvmProperty[] NameProperties => nameProperties;
        public override bool CanUseControlContentForName => false;
        protected override void AddDeclarationsCore(PageObjectDefinition pageObject, SeleniumGeneratorContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}