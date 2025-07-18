namespace FacebookLiveVoting.Services
{
	public class RoundService
	{
		private int _currentRound = 1;

		public int GetCurrentRound() => _currentRound;

		public void NextRound()
		{
			_currentRound++;
		}

		public void SetRound(int round)
		{
			_currentRound = round;
		}
	}
}
