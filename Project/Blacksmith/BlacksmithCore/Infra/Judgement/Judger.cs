using BlacksmithCore.Infra.DSL;
using BlacksmithCore.Infra.Judgement.Core;
using BlacksmithCore.Infra.Models.Entites;

namespace BlacksmithCore.Infra.Judgement
{
    public class Judger
    {
        public Community Player { get; protected set; }
        public Community Enemy { get; protected set; }
        public JudgeRuleManager JudgeRuleManager { get; }

        public Judger(Community player, Community enemy)
        {
            Player = player;
            Enemy = enemy;
            JudgeRuleManager = new();
        }

        public void Copy(Judger origin)
        {
            JudgeRuleManager.Copy(origin.JudgeRuleManager);
        }

        protected virtual IEnumerable<Intent> Compile(IEnumerable<IDSLSourceFile> sourceFiles)
        {
            int? totalCount = (sourceFiles as ICollection<IDSLSourceFile>)?.Count;
            var passives = new List<IDSLSourceFile>(totalCount ?? 4);
            var nonPassives = new List<IDSLSourceFile>(totalCount ?? 4);

            foreach (var sf in sourceFiles)
            {
                if (sf.IsPassive)
                    passives.Add(sf);
                else
                    nonPassives.Add(sf);
            }

            int passiveCount = passives.Count;
            int total = passiveCount + nonPassives.Count;
            var result = new Intent[total];
            Intent temp = new() { Execute = null! };

            // 1. 先放入被动文件的占位符（与原来一样）
            for (int i = 0; i < passiveCount; i++)
            {
                result[i] = temp;
            }

            // 2. 编译所有非被动文件并顺序放入被动占位符之后
            int index = passiveCount;
            foreach (var sf in nonPassives)
            {
                result[index++] = sf.Compile(JudgeRuleManager);
            }

            // 3. 最后编译被动文件并替换原来的占位符
            for (int i = 0; i < passiveCount; i++)
            {
                result[i] = passives[i].Compile(JudgeRuleManager);
            }

            return result;
        }

        public virtual void Judge(IEnumerable<IDSLSourceFile> playerSfs, IEnumerable<IDSLSourceFile> enemySfs)
        {
            foreach (var temp in Compile(playerSfs))
            {
                temp.Execute(Player);
            }
            foreach (var temp in Compile(enemySfs))
            {
                temp.Execute(Enemy);
            }
            JudgeRuleManager.Judge(Player, Enemy);
        }
    }
}
