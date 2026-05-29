using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DebugHeroFileDungeonRPG
{
    public static class SupabaseManager
    {
        // url
        private static readonly string BaseUrl = "https://eatzjlbrdvlohkqrswop.supabase.co/rest/v1/leaderboard";

        // API키
        private static readonly string AnonKey = "sb_publishable_pOTbCiY-wD9g1k755fwuWQ_6OQg1piB";

       
        // 보스 단계별 클리어 시점의 누적 플레이 시간(틱)과 사망 횟수를 클라우드로 전송합니다.
       
        public static async Task SendClearLogAsync(string name, int stageIndex, int ticks, int deaths)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("apikey", AnonKey);
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {AnonKey}");

                    string json = $"{{\"user_name\":\"{name}\",\"stage_index\":{stageIndex},\"clear_time_ticks\":{ticks},\"death_count\":{deaths}}}";
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    await client.PostAsync(BaseUrl, content);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Supabase 전송 에러: " + ex.Message);
            }
        }

      
        public static async Task<string> GetStageRankingsAsync(int stageIndex)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("apikey", AnonKey);
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {AnonKey}");

                    // HTTP 요청 무한 캐싱 차단 가드 
                    // 클라이언트 단의 로컬 메모리 캐시를 원천 무효화하고 무조건 실시간 Supabase Cloud 진짜 raw 데이터를 긁어오도록 강제합니다.
                    client.DefaultRequestHeaders.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue
                    {
                        NoCache = true,
                        NoStore = true
                    };

                    string queryUrl = $"{BaseUrl}?stage_index=eq.{stageIndex}&order=clear_time_ticks.asc,death_count.asc&limit=30";

                    HttpResponseMessage response = await client.GetAsync(queryUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        return await response.Content.ReadAsStringAsync();
                    }
                }
            }
            catch { }
            return "[]";
        }
    }
}