﻿using Newtonsoft.Json.Linq;
using OPCAIC.ApiService.ModelValidationHandling.Attributes;
using OPCAIC.Infrastructure;

namespace OPCAIC.ApiService.Models.Games
{
	public class UpdateGameModel
	{
		[ApiRequired]
		[ApiMaxLength(StringLengths.GameName)]
		public string Name { get; set; }

		[ApiRequired]
		[ApiMaxLength(StringLengths.GameKey)]
		public string Key { get; set; }

		[ApiRequired]
		public JObject ConfigurationSchema { get; set; }
	}
}