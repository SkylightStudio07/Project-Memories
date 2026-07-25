# Dayeon BGM DSP render manifest

이 폴더의 WAV는 `BeatMemories_Dayeon_BGM` 복제 씬 전용이다. 모두 48 kHz, stereo, PCM16이며 첫 음악 강박의 런타임 오프셋은 0초다. 반복은 `AudioSource.loop`가 아니라 `RhythmAudioController`가 메트로놈의 절대 DSP 박자표에 맞춰 A/B 소스로 예약한다.

| 출력 | 소스 구간 | BPM | 박 수 | 프레임 | 볼륨 | 경계 보정 |
|---|---:|---:|---:|---:|---:|---:|
| Stage_1_94BPM_Loop.wav | 기존 Stage 1 루프를 12.2 ms 회전 | 94 | 32 | 980,426 | 0.35 | 순환 OLA |
| Stage_2_97BPM_Loop.wav | 스테이지2.wav 1.39375–160.55875 s | 97 | 256 | 7,600,825 | 0.34 | 80 ms OLA + 5 ms endpoint |
| Stage_3_137BPM_Loop.wav | 스테이지3.wav 0.410–21.402 s | 137 | 48 | 1,009,051 | 0.40 | 20 ms OLA |
| Stage_4_93BPM_Loop.wav | 스테이지4.wav 2.240–28.124 s | 93 | 40 | 1,238,710 | 0.39 | 20 ms OLA + 5 ms endpoint |
| Boss_1_95BPM_Loop.wav | Boss_phase_1.wav 1.525–117.322 s | 95 | 184 | 5,578,105 | 0.39 | 35 ms OLA + 5 ms endpoint |
| Boss_2_92BPM_Loop.wav | Boss_phase_2.wav 0.965–149.317 s | 92 | 228 | 7,137,391 | 0.35 | 35 ms OLA |

각 소스는 지정 구간을 목표 프레임 수로 리타이밍한 뒤 순환 경계를 보정했다. Stage 2는 첫 강박과 256박 뒤의 다음 강박을 onset backtracking으로 다시 잡았다. 네 개의 16마디 구간이 각각 96.50–96.53 BPM으로 일치해, 64마디 전체를 하나의 96.503628 BPM 격자로 보고 97 BPM으로 VHQ 리샘플링했다. 최종 가청 판단은 복제 씬에서 메트로놈을 함께 재생해 시작·중간·마지막 마디와 강제 루프 경계를 확인한다.
