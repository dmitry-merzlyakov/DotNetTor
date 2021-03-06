﻿using System;
using System.Net.Http;
using System.Threading.Tasks;
using DotNetTor.Exceptions;
using System.Net;
using DotNetEssentials.Logging;

namespace DotNetTor.Example
{
	public class Program
	{
		// For proper configuraion see https://github.com/nopara73/DotNetTor
#pragma warning disable IDE1006 // Naming Styles
		private static async Task Main()
#pragma warning restore IDE1006 // Naming Styles
		{
			try
			{
				Logger.SetModes(LogMode.Console);
				Logger.SetMinimumLevel(LogLevel.Info);

				await DoARandomRequestAsync();
				await RequestWith3IpAsync();
				await CanRequestDifferentDomainsWithSameHandlerAsync();
			}
			catch (Exception ex)
			{
				Logger.LogCritical(ex, LogLevel.Debug, "");
			}

			Logger.LogInfo("Press a key to exit..");
			Console.ReadKey();
		}

		private static async Task DoARandomRequestAsync()
		{
			using (var httpClient = new HttpClient(new TorSocks5Handler(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9050))))
			{
				HttpResponseMessage message = await httpClient.GetAsync("http://api.qbit.ninja/whatisit/what%20is%20my%20future");
				var content = await message.Content.ReadAsStringAsync();
				Logger.LogInfo(content);
			}
		}

		private static async Task RequestWith3IpAsync()
		{
			var requestUri = "https://api.ipify.org/";

			// 1. Get real IP
			using (var httpClient = new HttpClient())
			{
				var message = await httpClient.GetAsync(requestUri);
				var content = await message.Content.ReadAsStringAsync();
				Logger.LogInfo($"Your real IP: \t\t{content}");
			}

			// 2. Get Tor IP
			using (var httpClient = new HttpClient(new TorSocks5Handler(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9050))))
			{
				var message = await httpClient.GetAsync(requestUri);
				var content = await message.Content.ReadAsStringAsync();
				Logger.LogInfo($"Your Tor IP: \t\t{content}");

				// 3. Change Tor IP
				var controlPortClient = new TorControlClient("127.0.0.1", controlPort: 9051, password: "ILoveBitcoin21");
				await controlPortClient.ChangeCircuitAsync();

				// 4. Get changed Tor IP
				message = await httpClient.GetAsync(requestUri);
				content = await message.Content.ReadAsStringAsync();
				Logger.LogInfo($"Your other Tor IP: \t{content}");
			}
		}
		private static async Task CanRequestDifferentDomainsWithSameHandlerAsync()
		{
			using (var httpClient = new HttpClient(new TorSocks5Handler(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9050))))
			{
				var message = await httpClient.GetAsync("https://api.ipify.org/");
				var content = await message.Content.ReadAsStringAsync();
				Logger.LogInfo($"Your Tor IP: \t\t{content}");

				message = await httpClient.GetAsync("http://api.qbit.ninja/whatisit/what%20is%20my%20future");
				content = await message.Content.ReadAsStringAsync();
				Logger.LogInfo(content);
			}
		}
	}
}