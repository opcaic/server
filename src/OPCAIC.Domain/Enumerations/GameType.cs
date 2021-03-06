﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using OPCAIC.Domain.Infrastructure;

namespace OPCAIC.Domain.Enums
{
	[TypeConverter(typeof(EnumerationConverter<GameType>))]
	public class GameType : Enumeration<GameType>
	{
		/// <summary>
		///     Games for one player.
		/// </summary>
		public static readonly GameType SinglePlayer = Create(new[]
		{
			TournamentFormat.SinglePlayer
		});

		/// <summary>
		///     Games for two players.
		/// </summary>
		public static readonly GameType TwoPlayer = Create(new[]
		{
			TournamentFormat.SingleElimination, TournamentFormat.DoubleElimination,
			TournamentFormat.Table, TournamentFormat.Elo
		});

		/// <summary>
		///     Games for N >= 3 players.
		/// </summary>
		public static readonly GameType MultiPlayer = Create(new[] {TournamentFormat.Elo});

		private readonly List<TournamentFormat> supportedFormats = new List<TournamentFormat>();

		/// <summary>
		///     List of formats this tournament type supports
		/// </summary>
		public IReadOnlyList<TournamentFormat> SupportedFormats => supportedFormats;

		private static GameType Create(TournamentFormat[] formats,
			[CallerMemberName] string name = null)
		{
			var type = Create<GameType>(name);
			type.supportedFormats.AddRange(formats);
			return type;
		}

		public bool SupportsTournamentFormat(TournamentFormat format)
		{
			return supportedFormats.Contains(format);
		}
	}
}