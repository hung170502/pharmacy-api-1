namespace Pharmacy_API.Supports
{
    public class RequestDtoBase
    {
        private string? _userID;

		public string? GetUserID() {
			return _userID;
		}

		public void SetUserID(string? userID)
		{
			_userID = userID;
		}

		public bool IsValid()
		{
			try
			{
				var ctx = new System.ComponentModel.DataAnnotations.ValidationContext(this);
				// will throw an exception if invalid
				System.ComponentModel.DataAnnotations.Validator.ValidateObject(this, ctx, true);
			}
			catch
			{
				return false;
			}

			return true;
		}
    }
}