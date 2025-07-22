using PhantomMaskAPI.Interfaces;
using PhantomMaskAPI.Models.DTOs;

namespace PhantomMaskAPI.Services
{
    public class RelevanceService : IRelevanceService
    {
        /// <summary>
        /// 內部使用的相關性分數計算方法。
        /// 此版本統一只使用 'Name' 進行匹配，不考慮 'Brand'。
        /// </summary>
        public double CalculateRelevanceScoreInternal(string query, RelevanceDto item)
        {
            double relevanceScore = 0.0;

            // --- 1. 正規化 ---
            string normalizedQuery = query.ToLowerInvariant().Trim();
            string normalizedItemName = item.Name.ToLowerInvariant().Trim();

            string[] queryWords = normalizedQuery.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // --- 2. 核心匹配分數 (基礎分) ---
            // 完全匹配
            if (normalizedItemName == normalizedQuery)
            {
                relevanceScore += 0.9;
            }

            // 包含匹配 (如果沒有完全匹配，則計算)
            if (relevanceScore < 0.9)
            {
                if (normalizedItemName.Contains(normalizedQuery) || normalizedQuery.Contains(normalizedItemName))
                {
                    relevanceScore += 0.6;
                }
            }

            // 多詞部分匹配
            if (queryWords.Length > 1)
            {
                int matchedWordsCount = 0;
                foreach (string qWord in queryWords)
                {
                    if (normalizedItemName.Contains(qWord))
                    {
                        matchedWordsCount++;
                    }
                }
                if (relevanceScore < 0.8 && matchedWordsCount > 0)
                {
                    relevanceScore += ((double)matchedWordsCount / queryWords.Length) * 0.4;
                }
            }

         

            // --- 5. 限制與截斷分數 ---
            relevanceScore = Math.Min(1.0, Math.Max(0.0, relevanceScore));

            return relevanceScore;
        }

    }
}
