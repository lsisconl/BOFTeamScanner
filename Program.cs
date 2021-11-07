using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Transactions;

namespace BOFTeamScanner
{
    class Program
    {
        static void Main(string[] args)
        {
            var matches = new Matches(File.ReadAllText("1.html"),
                @"(?<=<td id=""teamname""  title="").*?(?="">)",
                @"(?<=<td id=""memberlist"" >).[\D\W\S]*?(?=</td>)",
                @"(?<=<td id=""leader""  title="").*?(?="">)",
                @"(?<=<td id=""member"" >).*?(?=</td>)",
                @"(?<=<td id=""comment""  title="").[\D\W\S]*?(?="">)",
                @"(?<=<td id=""insert_time"" >).*?(?=</td>)",
                @"(?<=<td id=""update_time"" >).*?(?=</td>)",
                @"(?<=<td id=""musnum"" ><a href=""kprof.cgi\?mode=search\;search_musnum=).*?(?="">)",
                @"(?<=<td id="""" ><span class=""detailcheck""><a href="").*?(?="">)");
            var bofTeamInfos = new List<BofTeamInfo>();
            var i = 0;
            while (matches.TeamNameMatch.Success)
            {
                var bofTeamInfo = new BofTeamInfo(matches.TeamNameMatch.ToString(),
                    matches.TeamMembersMatch.ToString(),
                    matches.LeaderNameMatch.ToString(),
                    matches.MemberCountMatch.ToString(),
                    matches.CommentMatch.ToString(),
                    matches.InsertTimeMatch.ToString(),
                    matches.UpdateTimeMatch.ToString(),
                    matches.SongAmountMatch.ToString(), 
                    matches.TeamLinkMatch.ToString());
                matches = matches.Next(matches);
                bofTeamInfos.Add(bofTeamInfo);
            }
            foreach (var teamInfo in bofTeamInfos)
            {
                ++i;
                Console.WriteLine($"- 团队名称（TeamName）：{teamInfo.TeamName}\n" +
                                  $"团队成员（TeamMembers）：{teamInfo.TeamMembers}\n" +
                                  $"团队代表（TeamLeader）：{teamInfo.LeaderName}\n" +
                                  $"曲目数量（SongAmount）：{teamInfo.SongAmount}\n" +
                                  $"团队人数（MemberCount）：{teamInfo.MemberCount}\n" +
                                  $"团队留言（Comment）：{teamInfo.Comment}\n" +
                                  $"加入时间（InsertTime）：{teamInfo.InsertTime}\n" +
                                  $"更改日期（UpdateTime）：{teamInfo.UpdateTime}\n" +
                                  $"队伍链接（TeamLink）：https://manbow.nothing.sh/event/page/bofxvii/teamlist21/{ teamInfo.TeamLink }\n");
            }

            Console.WriteLine(i);

        }
        
        public static string HttpGet(string url)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9";
            var response = (HttpWebResponse)request.GetResponse();
            var myResponseStream = response.GetResponseStream();
            if (myResponseStream != null)
            {
                var myStreamReader = new StreamReader(myResponseStream, Encoding.Default);
                var retString = myStreamReader.ReadToEnd();
                myStreamReader.Close();
                myResponseStream.Close();
                return ConvertExtendedASCII(retString);
            }

            return null;
        }
        
        public static string ConvertExtendedASCII(string sourceHtmlTest)
        {
            string result = string.Empty;
            try
            {
                StringBuilder str = new StringBuilder();
                char c;
                for (int i = 0; i < sourceHtmlTest.Length; i++)
                {
                    c = sourceHtmlTest[i];
                    if (Convert.ToInt32(c) > 127)
                    {
                        str.Append("&#" + Convert.ToInt32(c) + ";");
                    }
                    else
                    {
                        str.Append(c);
                    }
                }
                result = str.ToString();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
            return result;
        }
    }

    public class BofTeamInfo
    {
        public readonly string TeamName;

        public readonly string TeamMembers;

        public readonly string LeaderName;

        public readonly string MemberCount;

        public readonly string Comment;

        public readonly string InsertTime;

        public readonly string UpdateTime;

        public readonly string SongAmount;

        public readonly string TeamLink;

        public BofTeamInfo(string teamName,string teamMembers,string leaderName,string memberCount,string comment,string insertTime,string updateTime,string songAmount,string teamLink)
        {
            TeamName = teamName;
            TeamMembers = teamMembers;
            LeaderName = leaderName;
            MemberCount = memberCount;
            Comment = comment;
            InsertTime = insertTime;
            UpdateTime = updateTime;
            SongAmount = songAmount;
            TeamLink = teamLink;
        }
    }

    public class Matches
    {
        public Match TeamNameMatch;
        public Match TeamMembersMatch;
        public Match LeaderNameMatch;
        public Match MemberCountMatch;
        public Match CommentMatch;
        public Match InsertTimeMatch;
        public Match UpdateTimeMatch;
        public Match SongAmountMatch;
        public Match TeamLinkMatch;

        public Matches(string htmlRawText,string teamNameMatchRule,string teamMembersMatchRule,string leaderNameMatchRules,string memberCountMatchRules,string commentMatchRules,string insertTimeMatchRules,string updateTimeMatchRules,string songAmountMatchRules,string teamLinkMatchRules)
        {
            TeamNameMatch = Regex.Match(htmlRawText,teamNameMatchRule);
            TeamMembersMatch = Regex.Match(htmlRawText, teamMembersMatchRule);
            LeaderNameMatch = Regex.Match(htmlRawText, leaderNameMatchRules);
            MemberCountMatch = Regex.Match(htmlRawText, memberCountMatchRules);
            CommentMatch = Regex.Match(htmlRawText, commentMatchRules);
            InsertTimeMatch = Regex.Match(htmlRawText, insertTimeMatchRules);
            UpdateTimeMatch = Regex.Match(htmlRawText, updateTimeMatchRules);
            SongAmountMatch = Regex.Match(htmlRawText, songAmountMatchRules);
            TeamLinkMatch = Regex.Match(htmlRawText,teamLinkMatchRules);
        }

        public Matches Next(Matches matches)
        {
            
            matches.CommentMatch = matches.CommentMatch.NextMatch();
            matches.InsertTimeMatch = matches.InsertTimeMatch.NextMatch();
            matches.LeaderNameMatch = matches.LeaderNameMatch.NextMatch();
            matches.MemberCountMatch = matches.MemberCountMatch.NextMatch();
            matches.SongAmountMatch = matches.SongAmountMatch.NextMatch();
            matches.TeamMembersMatch = matches.TeamMembersMatch.NextMatch();
            matches.TeamNameMatch = matches.TeamNameMatch.NextMatch();
            matches.UpdateTimeMatch = matches.UpdateTimeMatch.NextMatch();
            matches.TeamLinkMatch = matches.TeamLinkMatch.NextMatch();
            return matches;
        }
    }
    
    
}
