namespace Raindrop.DS
{
    internal interface ICache
	{
		T Get<T>(string key);
		void Set<T>(string key, T value);
		//void SetSliding<T>(string key, T value);
		//void Set<T>(string key, T value, int duration);
		//void SetSliding<T>(string key, T value, int duration);
		//void Set<T>(string key, T value, DateTimeOffset expiration);
		bool Exists(string key);
		void Remove(string key);
	}
}