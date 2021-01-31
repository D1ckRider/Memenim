﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows;
using RIS;

namespace Memenim.Localization.Entities
{
    public class LocalizationXamlFile
    {
        public string Path { get; private set; }
        public string Name { get; private set; }
        public string Extension { get; private set; }

        public ResourceDictionary Dictionary { get; private set; }
        public string ElementName { get; private set; }
        public string DictionaryName { get; private set; }
        public string LocaleName { get; private set; }

        public CultureInfo Culture { get; private set; }
        public string CultureName { get; private set; }

        public LocalizationXamlFile(string path,
            string elementName)
        {
            Load(path, elementName);
        }

        private void Load(string path, string elementName)
        {
            if (!System.IO.Path.IsPathRooted(path))
            {
                var exception = new ArgumentException(
                    $"Path['{path}'] must contain the root",
                    nameof(path));
                Events.OnError(new RErrorEventArgs(exception,
                    exception.Message, exception.StackTrace));
                throw exception;
            }
            if (System.IO.Path.GetFileName(path) == null)
            {
                var exception = new ArgumentException(
                    $"Path['{path}'] must refer to the file",
                    nameof(path));
                Events.OnError(new RErrorEventArgs(exception,
                    exception.Message, exception.StackTrace));
                throw exception;
            }
            if (!File.Exists(path))
            {
                var exception = new FileNotFoundException(
                    $"File '{path}' not found");
                Events.OnError(new RErrorEventArgs(exception,
                    exception.Message, exception.StackTrace));
                throw exception;
            }

            Path = path;

            if (string.IsNullOrEmpty(elementName))
            {
                var exception = new ArgumentException(
                    "Element name must not be null or empty",
                    nameof(elementName));
                Events.OnError(new RErrorEventArgs(exception,
                    exception.Message, exception.StackTrace));
                throw exception;
            }

            ElementName = elementName;

            var extension = System.IO.Path.GetExtension(path);

            if (string.IsNullOrEmpty(extension))
            {
                var exception = new ArgumentException(
                    $"File['{path}'] extension must not be null or empty",
                    nameof(path));
                Events.OnError(new RErrorEventArgs(exception,
                    exception.Message, exception.StackTrace));
                throw exception;
            }
            if (extension != ".xaml")
            {
                var exception = new ArgumentException(
                    $"File['{path}'] must have an extension '.xaml'",
                    nameof(path));
                Events.OnError(new RErrorEventArgs(exception,
                    exception.Message, exception.StackTrace));
                throw exception;
            }

            Extension = extension;

            var name = System.IO.Path.GetFileNameWithoutExtension(path);

            if (string.IsNullOrEmpty(name))
            {
                var exception = new ArgumentException(
                    $"File['{path}'] name must not be null or empty",
                    nameof(path));
                Events.OnError(new RErrorEventArgs(exception,
                    exception.Message, exception.StackTrace));
                throw exception;
            }
            if (!name.StartsWith($"{elementName}."))
            {
                var exception = new ArgumentException(
                    $"File['{path}'] name must be in the format [element name + '.' + culture name] " +
                    $"(In this case - ['{elementName}.' + culture name]) " +
                    "(culture name must be in the ISO 639 format (for example, 'en-US' or 'ru-RU'))",
                    nameof(path));
                Events.OnError(new RErrorEventArgs(exception,
                    exception.Message, exception.StackTrace));
                throw exception;
            }

            Name = name;

            var separatorIndex = name.IndexOf('.');
            var cultureName = name[(separatorIndex + 1)..];
            CultureInfo culture;

            try
            {
                culture = CultureInfo.GetCultureInfo(cultureName);
            }
            catch (Exception)
            {
                var exception = new ArgumentException(
                    $"Culture named '{cultureName}' for file['{path}'] not found",
                    nameof(path));
                Events.OnError(new RErrorEventArgs(exception,
                    exception.Message, exception.StackTrace));
                throw exception;
            }

            Culture = culture;
            CultureName = cultureName;

            ResourceDictionary dictionary;

            try
            {
                dictionary = new ResourceDictionary
                {
                    Source = new Uri(path)
                };
            }
            catch (Exception ex)
            {
                Events.OnError(new RErrorEventArgs(ex, ex.Message, ex.StackTrace));
                throw;
            }

            Dictionary = dictionary;

            if (!dictionary.Contains("ResourceDictionaryName"))
            {
                var exception = new KeyNotFoundException(
                    $"Dictionary file['{path}'] does not contain 'ResourceDictionaryName' definition");
                Events.OnError(new RErrorEventArgs(exception,
                    exception.Message, exception.StackTrace));
                throw exception;
            }

            string dictionaryName = dictionary["ResourceDictionaryName"].ToString();

            if (string.IsNullOrWhiteSpace(dictionaryName))
            {
                var exception = new Exception(
                    $"ResourceDictionaryName value in file['{path}'] must not be null or empty");
                Events.OnError(new RErrorEventArgs(exception,
                    exception.Message, exception.StackTrace));
                throw exception;
            }
            if (dictionaryName != "localization-xaml")
            {
                var exception = new Exception(
                    $"The dictionary file['{path}'] is not a localization dictionary " +
                    "(The ResourceDictionaryName value must be 'localization-xaml')");
                Events.OnError(new RErrorEventArgs(exception,
                    exception.Message, exception.StackTrace));
                throw exception;
            }

            DictionaryName = dictionaryName;

            if (!dictionary.Contains("ResourceCultureName"))
            {
                var exception = new KeyNotFoundException(
                    $"Dictionary file['{path}'] does not contain 'ResourceCultureName' definition");
                Events.OnError(new RErrorEventArgs(exception,
                    exception.Message, exception.StackTrace));
                throw exception;
            }

            string dictionaryCultureName = dictionary["ResourceCultureName"].ToString();

            if (string.IsNullOrWhiteSpace(dictionaryCultureName))
            {
                var exception = new Exception(
                    $"ResourceCultureName value in file['{path}'] must not be null or empty");
                Events.OnError(new RErrorEventArgs(exception,
                    exception.Message, exception.StackTrace));
                throw exception;
            }
            if (dictionaryCultureName != cultureName)
            {
                var exception = new Exception(
                    $"Dictionary file['{path}'] is not intended for the culture that is declared in its name " +
                    $"('{dictionaryCultureName}' is not equal to '{cultureName}')");
                Events.OnError(new RErrorEventArgs(exception,
                    exception.Message, exception.StackTrace));
                throw exception;
            }

            if (!dictionary.Contains("ResourceLocaleName"))
            {
                var exception = new KeyNotFoundException(
                    $"Dictionary file['{path}'] does not contain 'ResourceLocaleName' definition");
                Events.OnError(new RErrorEventArgs(exception,
                    exception.Message, exception.StackTrace));
                throw exception;
            }

            string dictionaryLocaleName = dictionary["ResourceLocaleName"].ToString();

            if (string.IsNullOrWhiteSpace(dictionaryLocaleName))
            {
                var exception = new Exception(
                    $"ResourceLocaleName value in file['{path}'] must not be null or empty");
                Events.OnError(new RErrorEventArgs(exception,
                    exception.Message, exception.StackTrace));
                throw exception;
            }

            LocaleName = dictionaryLocaleName;
        }
    }
}