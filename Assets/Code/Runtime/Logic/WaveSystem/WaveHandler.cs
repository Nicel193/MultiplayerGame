using Code.Runtime.Configs;
using Code.Runtime.Logic.Enemies;
using Code.Runtime.UI;
using Fusion;
using UnityEngine;
using Zenject;

namespace Code.Runtime.Logic.WaveSystem
{
    public class WaveHandler : NetworkBehaviour, IWaveHandler
    {
        [Networked] public TickTimer WaveTimer { get; set; }

        [SerializeField] private WaveConfig[] waveConfigs;
        [SerializeField] private WaveTimeTextView waveTimeTextView;

        private IEnemyFactory _enemyFactory;
        private INetworkPlayersHandler _networkPlayersHandler;
        private WaveStateMachine _waveStateMachine;
        private int _currentWaveIndex;

        [Inject]
        private void Construct(IEnemyFactory enemyFactory, INetworkPlayersHandler networkPlayersHandler)
        {
            _networkPlayersHandler = networkPlayersHandler;
            _enemyFactory = enemyFactory;
        }

        public void Initialize()
        {
            WaveConfig waveConfig = waveConfigs[_currentWaveIndex];

            _waveStateMachine = new WaveStateMachine();

            _waveStateMachine.RegisterState(new WaveBreakState(this, _waveStateMachine));
            _waveStateMachine.RegisterState(new WaveSpawnState(this, _enemyFactory, _networkPlayersHandler,
                _waveStateMachine));

            _waveStateMachine.Enter<WaveBreakState, WaveConfig>(waveConfig);
        }

        public WaveConfig GetNextWaveConfig()
        {
            _currentWaveIndex++;

            return waveConfigs[_currentWaveIndex];
        }

        public override void FixedUpdateNetwork()
        {
            UpdateTimerText();

            _waveStateMachine.UpdateState();
        }

        private void UpdateTimerText()
        {
            float? remainingTime = WaveTimer.RemainingTime(Runner);

            if (remainingTime != null)
                waveTimeTextView.RPC_UpdateTimer(remainingTime.Value);
        }
    }
}