﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Coypu.Drivers.Selenium;
using NSpec;

namespace Coypu.Drivers.Tests
{
	public class Test_each_driver_and_browser_combination : nspec
	{
		private const string INTERACTION_TESTS_PAGE = @"html\InteractionTestsPage.htm";
		Driver driver;

		public void when_testing_each_driver()
		{
			LoadSpecsFor(typeof(SeleniumWebDriver));
		}

		private void LoadSpecsForEachBrowser(Type driverType)
		{
			LoadSpecsFor(driverType, Browser.Firefox);
			LoadSpecsFor(driverType, Browser.Chrome);
			//LoadSpecsFor(driverType, Browser.InternetExplorer);
		}

		private void LoadDriverSpecs(Type driverType, Browser browser)
		{
			before = () => LoadTestHTML(driverType, browser);

			Assembly.GetExecutingAssembly().GetTypes()
					.Where(t => t.IsClass && typeof (DriverSpecs).IsAssignableFrom(t))
					.Do(LoadSpecs);
		}

		private void LoadSpecs(Type driverSpecsType)
		{
			describe[driverSpecsType.Name.ToLowerInvariant().Replace('_', ' ')] 
				= ((DriverSpecs)Activator.CreateInstance(driverSpecsType)).Specs(GetDriver, it);
		}

		private Driver GetDriver()
		{
			return driver;
		}

		private void LoadTestHTML(Type driverType, Browser browser)
		{
			Console.WriteLine("LoadTestHTML");
			EnsureDriver(driverType, browser);
			driver.Visit(GetTestHTMLPathLocation());
		}

		private void LoadSpecsFor(Type driverType)
		{
			context["and the driver is " + driverType.Name] = () => LoadSpecsForEachBrowser(driverType);
		}

		private void LoadSpecsFor(Type driverType, Browser browser)
		{
			context["and the browser is " + browser] = () => LoadDriverSpecs(driverType, browser);
		}

		private void EnsureDriver(Type driverType, Browser browser)
		{
			if (driver != null && driverType == driver.GetType() && Configuration.Browser == browser)
				return;

			if (driver != null)
				driver.Dispose();

			Configuration.Browser = browser;
			driver = (Driver)Activator.CreateInstance(driverType);
		}

		private string GetTestHTMLPathLocation()
		{
			var assemblyDirectory = new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName;
			var projRoot = Path.Combine(assemblyDirectory,@"..\..\");
			return new FileInfo(Path.Combine(projRoot,INTERACTION_TESTS_PAGE)).FullName;
		}
	}
}