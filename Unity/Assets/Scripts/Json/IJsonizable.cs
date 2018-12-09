using Matt.Json;

namespace Jake.Json
{
	interface IJsonizable
	{
		JSONObject Jsonize();
		void Dejsonize(JSONObject json);
	}
}
