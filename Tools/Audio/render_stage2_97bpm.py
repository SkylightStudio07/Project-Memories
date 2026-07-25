"""Render the Dayeon Stage 2 loop against the 97 BPM DSP grid.

The source has a two-beat pickup.  The loop starts at the backed-up onset at
1.39375 s and ends at the corresponding onset 256 beats later.  The source
section measures 96.503 BPM and is resampled to the 97 BPM DSP grid.
"""

from __future__ import annotations

import argparse
from pathlib import Path

import numpy as np
import soundfile as sf
import soxr


SAMPLE_RATE = 48_000
CHANNELS = 2
LOOP_BEATS = 256
TARGET_BPM = 97.0
TARGET_FRAMES = 7_600_825
SOURCE_START_FRAME = 66_900
SOURCE_END_FRAME = 7_706_820
BOUNDARY_CROSSFADE_FRAMES = 3_840
ENDPOINT_CORRECTION_FRAMES = 240


def raised_cosine(length: int) -> np.ndarray:
    phase = np.linspace(0.0, np.pi, length, dtype=np.float64)
    return (0.5 - 0.5 * np.cos(phase)).astype(np.float32)


def read_source(path: Path) -> np.ndarray:
    audio, sample_rate = sf.read(
        path,
        dtype="float32",
        always_2d=True,
    )
    if sample_rate != SAMPLE_RATE:
        raise ValueError(
            f"Expected {SAMPLE_RATE} Hz source, got {sample_rate} Hz."
        )
    if audio.shape[1] != CHANNELS:
        raise ValueError(
            f"Expected {CHANNELS} source channels, got {audio.shape[1]}."
        )
    if audio.shape[0] < SOURCE_END_FRAME:
        raise ValueError(
            f"Source has {audio.shape[0]} frames; "
            f"{SOURCE_END_FRAME} are required."
        )
    return audio


def stretch_loop(source: np.ndarray) -> np.ndarray:
    segment = source[SOURCE_START_FRAME:SOURCE_END_FRAME]
    rendered = soxr.resample(
        segment,
        segment.shape[0],
        TARGET_FRAMES,
        quality="VHQ",
    )
    if rendered.shape[0] != TARGET_FRAMES:
        raise ValueError(
            f"Resampler returned {rendered.shape[0]} frames."
        )
    return rendered.T.astype(np.float32, copy=False)


def make_preroll(source: np.ndarray) -> np.ndarray:
    source_loop_frames = SOURCE_END_FRAME - SOURCE_START_FRAME
    source_preroll_frames = round(
        BOUNDARY_CROSSFADE_FRAMES
        * source_loop_frames
        / TARGET_FRAMES
    )
    preroll = source[
        SOURCE_START_FRAME - source_preroll_frames:SOURCE_START_FRAME
    ]
    rendered = soxr.resample(
        preroll,
        source_loop_frames,
        TARGET_FRAMES,
        quality="VHQ",
    )
    if rendered.shape[0] < BOUNDARY_CROSSFADE_FRAMES:
        rendered = np.pad(
            rendered,
            ((BOUNDARY_CROSSFADE_FRAMES - rendered.shape[0], 0), (0, 0)),
            mode="edge",
        )
    return rendered[-BOUNDARY_CROSSFADE_FRAMES:].T.astype(
        np.float32,
        copy=False,
    )


def repair_loop_boundary(
    rendered: np.ndarray,
    preroll: np.ndarray,
) -> None:
    blend = raised_cosine(BOUNDARY_CROSSFADE_FRAMES)
    tail = rendered[:, -BOUNDARY_CROSSFADE_FRAMES:]
    tail *= 1.0 - blend
    tail += preroll * blend

    endpoint_blend = raised_cosine(ENDPOINT_CORRECTION_FRAMES)
    delta = rendered[:, :1] - rendered[:, -1:]
    rendered[:, -ENDPOINT_CORRECTION_FRAMES:] += (
        delta * endpoint_blend
    )


def validate(rendered: np.ndarray) -> None:
    if rendered.shape != (CHANNELS, TARGET_FRAMES):
        raise ValueError(f"Unexpected render shape: {rendered.shape}.")
    expected_frames = (
        LOOP_BEATS * 60.0 / TARGET_BPM * SAMPLE_RATE
    )
    if abs(TARGET_FRAMES - expected_frames) >= 0.6:
        raise ValueError(
            f"Target frame contract differs by "
            f"{TARGET_FRAMES - expected_frames:.3f} frames."
        )
    if not np.all(np.isfinite(rendered)):
        raise ValueError("Render contains non-finite samples.")
    peak = float(np.max(np.abs(rendered)))
    if peak > 1.0:
        rendered /= peak
    boundary_jump = float(
        np.max(np.abs(rendered[:, 0] - rendered[:, -1]))
    )
    if boundary_jump > 1.0 / 32768.0:
        raise ValueError(
            f"Loop boundary jump is {boundary_jump:.8f}."
        )


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("source", type=Path)
    parser.add_argument("output", type=Path)
    args = parser.parse_args()

    source = read_source(args.source)
    rendered = stretch_loop(source)
    preroll = make_preroll(source)
    repair_loop_boundary(rendered, preroll)
    validate(rendered)

    args.output.parent.mkdir(parents=True, exist_ok=True)
    sf.write(
        args.output,
        rendered.T,
        SAMPLE_RATE,
        subtype="PCM_16",
    )

    source_frames = SOURCE_END_FRAME - SOURCE_START_FRAME
    source_bpm = (
        LOOP_BEATS * 60.0 * SAMPLE_RATE / source_frames
    )
    print(f"source frames: {source_frames}")
    print(f"source BPM: {source_bpm:.6f}")
    print(f"target frames: {TARGET_FRAMES}")
    print(
        "boundary jump: "
        f"{np.max(np.abs(rendered[:, 0] - rendered[:, -1])):.8f}"
    )
    print(f"peak: {np.max(np.abs(rendered)):.6f}")


if __name__ == "__main__":
    main()
