using System;

namespace OPCAIC.Application.Dtos.Users
{
	public class UserReferenceDto : IEquatable<UserReferenceDto>
	{
		public long Id { get; set; }

		public string Username { get; set; }

		public string Email { get; set; }

		public string Organization { get; set; }

		/// <inheritdoc />
		public bool Equals(UserReferenceDto other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}

			return Id == other.Id;
		}

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
			{
				return false;
			}

			if (ReferenceEquals(this, obj))
			{
				return true;
			}

			if (obj.GetType() != this.GetType())
			{
				return false;
			}

			return Equals((UserReferenceDto) obj);
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			return Id.GetHashCode();
		}

		public static bool operator ==(UserReferenceDto left, UserReferenceDto right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(UserReferenceDto left, UserReferenceDto right)
		{
			return !Equals(left, right);
		}
	}
}