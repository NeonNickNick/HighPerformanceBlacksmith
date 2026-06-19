using System.Collections.Concurrent;
using BlacksmithCore.Infra.Judgement;
using BlacksmithCore.Infra.Models.Components;
using BlacksmithCore.Infra.Models.Entites;
using BlacksmithCore.Infra.Profession;

namespace BlacksmithCore.Driver
{
    public class DefaultSkillContext : ISkillContext
    {
        public ISudoOperations SudoOperations { get; }
        public string SkillName { get; }
        public Community Self { get; }
        public int Param { get; }
        public string StringParam { get; }
        public (string SkillName, int Param, string StringParam) History
            => (SkillName, Param, StringParam);
        public DefaultSkillContext(ISudoOperations sudoOperations, string skillName, Community self, int param, string stringParam)
        {
            SudoOperations = sudoOperations;
            SkillName = skillName;
            Self = self;
            Param = param;
            StringParam = stringParam;
        }
    }

    public static class GameInstancePool
    {
        private static readonly ConcurrentBag<GameInstance> _pool = new();

#if DEBUG
        /// <summary>每 N 次 Rent 打印一次池中实例数量，用于排查泄露。</summary>
        private const int DiagnosticInterval = 100_000;

        /// <summary>线程安全的 Rent 累计计数。</summary>
        private static int _rentCount;
#endif

        /// <summary>当前池中缓存的实例数量（近似快照）。</summary>
        public static int Count => _pool.Count;

        /// <summary>
        /// 从池中租用一个 GameInstance。池为空时创建新实例。
        /// </summary>
        public static GameInstance Rent()
        {
            if (_pool.TryTake(out var instance))
            {
                return instance;
            }
            return new GameInstance();
        }

        /// <summary>
        /// 将 GameInstance 归还到池中。
        /// 线程安全：ConcurrentBag 保证多线程并发归还的安全性。
        /// </summary>
        public static void Return(GameInstance instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }
            _pool.Add(instance);
        }

        /// <summary>
        /// 清空池中所有缓存的实例（用于测试或内存压力场景）。
        /// </summary>
        public static void Clear()
        {
            while (_pool.TryTake(out _)) { }
        }

        /// <summary>
        /// 供 DeepCopy 调用：Rent 后自动推进诊断计数器（仅在 DEBUG 配置下生效）。
        /// </summary>
        internal static GameInstance RentWithDiagnostic()
        {
            var instance = Rent();
#if DEBUG
            var count = Interlocked.Increment(ref _rentCount);
            if (count % DiagnosticInterval == 0)
            {
                Console.WriteLine($"[GameInstancePool] Rent 累计={count}, 池中实例={_pool.Count}");
            }
#endif
            return instance;
        }
    }

    public partial class GameInstance : ISudoOperations
    {
        public Community Player { get; private set; }
        public Community Enemy { get; private set; }
        public Judger Judger { get; private set; }
        public GameHistory History { get; private set; }
        public GameMetadata Metadata { get; private set; }
        public GameInstance()
        {
            Player = new(true);
            Enemy = new(false);
            Judger = new(Player, Enemy);
            History = new();
            Metadata = new();
        }
        public bool IsPlayer(Community community)
        {
            return community == Player;
        }
        public IReadOnlyList<((string SkillName, int Param, string StringParam), (string SkillName, int Param, string StringParam))> SkillHistory
            => History.SkillHistory;
        public GameInstance DeepCopy()
        {
            GameInstance res = GameInstancePool.RentWithDiagnostic();
            res.Copy(this);

            return res;
        }

        /// <summary>
        /// 将此实例归还到对象池。归还后不应再使用此实例。
        /// </summary>
        public void ReturnToPool()
        {
            GameInstancePool.Return(this);
        }

        public ICompileTimeMetadata CompileTimeMetadata => Metadata;
        public SkillDeclareResult TryDeclare(string skillName, int param, string stringParam = "")
        {
            DefaultSkillContext context = new(this, skillName, Player, param, stringParam);
            return Player.Focus.Get<Skill>().TryDeclare(skillName, context);
        }
        public SkillDeclareResult ETryDeclare(string skillName, int param, string stringParam = "")
        {
            DefaultSkillContext context = new(this, skillName, Enemy, param, stringParam);
            return Enemy.Focus.Get<Skill>().TryDeclare(skillName, context);
        }

        public void Declare(string skillName, int param, string esn, int ep, string stringParam = "", string esp = "")
        {
            Metadata.UpdateCurrentSkill(skillName, esn);
            var playerContext = new DefaultSkillContext(this, skillName, Player, param, stringParam);
            var enemyContext = new DefaultSkillContext(this, esn, Enemy, ep, esp);

            var ps = Player.Focus.Get<Skill>();
            var psfs = ps.GetPassiveSkill(playerContext);
            foreach (var s in Player.SummonList)
            {
                psfs.AddRange(s.Get<Skill>().GetPassiveSkill(playerContext));
            }
            psfs.Add(ps.Declare(skillName, playerContext));

            var es = Enemy.Focus.Get<Skill>();
            var esfs = es.GetPassiveSkill(enemyContext);
            foreach (var s in Enemy.SummonList)
            {
                esfs.AddRange(s.Get<Skill>().GetPassiveSkill(playerContext));
            }
            esfs.Add(es.Declare(esn, enemyContext));

            Judger.Judge(psfs, esfs);
            History.SkillHistory.Add((playerContext.History, enemyContext.History));
        }
    }
}
