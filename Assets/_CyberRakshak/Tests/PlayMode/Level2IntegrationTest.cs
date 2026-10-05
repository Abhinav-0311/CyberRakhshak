using System.Collections;
using System.Linq;
using CyberRakshak.PATCH;
using CyberRakshak.Runtime;
using MazeGenerator;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace CyberRakshak.Tests
{
    public sealed class Level2IntegrationTest
    {
        [UnityTest]
        public IEnumerator Level2_PlayerMotorTraversesBakedMazeAndReturnsWithoutChangingTrainingProgress()
        {
            string[] keys = { "CyberRakshak.TutorialComplete", "CyberRakshak.Level1Complete" };
            int[] saved = keys.Select(k => PlayerPrefs.GetInt(k, -1)).ToArray();
            string latest = GameProgression.LatestUnlockedScene;
            try
            {
                SceneManager.LoadScene("Game_Level02");
                yield return null;
                var player = GameObject.FindGameObjectWithTag("Player").GetComponent<CharacterController>();
                var motor = player.GetComponent("StarterAssets.ThirdPersonController") as Behaviour;
                var inputs = player.GetComponent("StarterAssets.StarterAssetsInputs");
                Assert.That(motor, Is.Not.Null);
                Assert.That(inputs, Is.Not.Null);
                Assert.That(Camera.main, Is.Not.Null);
                Assert.That(Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length, Is.EqualTo(1));
                Assert.That(player.GetComponent<SceneFallRecovery>(), Is.Not.Null);
                Assert.That(Object.FindFirstObjectByType<SceneMusicPlayer>().GetComponent<AudioSource>().isPlaying, Is.True);
                Assert.That(GameObject.Find("MazeObjective").GetComponent<Text>().text, Does.Contain("FIND THE EXIT"));
                PatchDialoguePresenter.Ensure().Hide();

                var move = inputs.GetType().GetField("move");
                var look = inputs.GetType().GetField("look");
                move.SetValue(inputs, Vector2.zero);
                look.SetValue(inputs, Vector2.zero);
                var pause = Object.FindFirstObjectByType<GameplayPauseController>();
                pause.Pause();
                Assert.That(Time.timeScale, Is.Zero);
                Assert.That(Cursor.lockState, Is.EqualTo(CursorLockMode.None));
                pause.OpenSettings();
                GameObject.Find("SettingsBackHit").GetComponent<Button>().onClick.Invoke();
                Assert.That(pause.IsPaused, Is.True);
                pause.Resume();
                Assert.That(Time.timeScale, Is.EqualTo(1f));
                yield return new WaitForSeconds(.3f);

                var generator = Object.FindFirstObjectByType<MazeGeneratorBehaviour>();
                var grid = MazeGeneratorService.Generate(generator.Parameters);
                var openings = MazeOpeningFinder.FindOpenings(grid);
                Assert.That(openings.Count, Is.EqualTo(2));
                var path = MazeAStarSolver.Solve(grid, openings[0], openings[1]).Path;
                Assert.That(path.Count, Is.GreaterThan(2));
                Vector3 spawn = player.transform.position;
                var start = generator.transform.TransformPoint(new Vector3((path[0].x + .5f) * generator.Parameters.CellSize,
                    0f, (path[0].y + .5f) * generator.Parameters.CellSize));
                Assert.That(Vector2.Distance(new Vector2(spawn.x, spawn.z), new Vector2(start.x, start.z)), Is.LessThan(.1f));
                Assert.That(player.isGrounded, Is.True);
                player.enabled = false;
                player.transform.position += Vector3.down * 12f;
                player.enabled = true;
                yield return null;
                yield return null;
                Assert.That(Vector3.Distance(player.transform.position, spawn), Is.LessThan(.2f), "Fall recovery must return to the entrance.");

                // Drive the real Starter Assets motor, not teleports or direct CharacterController.Move.
                for (int i = 1; i < path.Count; i++)
                {
                    Vector3 target = generator.transform.TransformPoint(new Vector3((path[i].x + .5f) * generator.Parameters.CellSize,
                        0f, (path[i].y + .5f) * generator.Parameters.CellSize));
                    float deadline = Time.time + 7f;
                    while (Time.time < deadline)
                    {
                        Vector3 delta = target - player.transform.position;
                        delta.y = 0f;
                        if (delta.magnitude < .18f) break;
                        Vector3 cameraRelative = Quaternion.Euler(0f, -Camera.main.transform.eulerAngles.y, 0f) * delta.normalized;
                        move.SetValue(inputs, new Vector2(cameraRelative.x, cameraRelative.z));
                        yield return null;
                    }
                    move.SetValue(inputs, Vector2.zero);
                    Assert.That(new Vector2(player.transform.position.x - target.x, player.transform.position.z - target.z).magnitude,
                        Is.LessThan(.25f), "Baked maze cell " + path[i]);
                }
                move.SetValue(inputs, Vector2.zero);
                Assert.That(GameObject.Find("CompletionMessage").GetComponent<Text>().text, Does.Contain("MAZE EXIT REACHED"));
                yield return new WaitForSecondsRealtime(2.2f);
                Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("LevelSelect"));
                for (int i = 0; i < keys.Length; i++)
                    Assert.That(PlayerPrefs.GetInt(keys[i], -1), Is.EqualTo(saved[i]), "Maze must not complete Tutorial/Level 1.");
                Assert.That(GameProgression.LatestUnlockedScene, Is.EqualTo(latest));
            }
            finally
            {
                Time.timeScale = 1f;
                Object.FindFirstObjectByType<PatchDialoguePresenter>()?.Hide();
            }
        }
    }
}
