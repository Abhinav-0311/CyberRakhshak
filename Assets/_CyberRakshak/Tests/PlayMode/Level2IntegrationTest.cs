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
        public IEnumerator Level2_CameraOrbitsStayClearOfWallsAndRecoverAfterFall()
        {
            SceneManager.LoadScene("Game_Level02");
            yield return null;
            yield return new WaitForSeconds(.3f);
            PatchDialoguePresenter.Ensure().Hide();
            var player = GameObject.FindGameObjectWithTag("Player").GetComponent<CharacterController>();
            var motor = player.GetComponent("StarterAssets.ThirdPersonController");
            var type = motor.GetType();
            var inputs = player.GetComponent("StarterAssets.StarterAssetsInputs");
            inputs.GetType().GetField("look").SetValue(inputs, Vector2.zero);
            type.GetField("LockCameraPosition").SetValue(motor, true);
            const System.Reflection.BindingFlags fields = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
            var yaw = type.GetField("_cinemachineTargetYaw", fields);
            var pitch = type.GetField("_cinemachineTargetPitch", fields);
            var generator = Object.FindFirstObjectByType<MazeGeneratorBehaviour>();
            var grid = MazeGeneratorService.Generate(generator.Parameters);
            var openings = MazeOpeningFinder.FindOpenings(grid);
            var path = MazeAStarSolver.Solve(grid, openings[0], openings[1]).Path;
            Vector3 spawn = player.transform.position;
            try
            {
                foreach (int index in new[] { 0, 3, path.Count / 2 })
                {
                    Vector3 position = generator.transform.TransformPoint(new Vector3((path[index].x + .5f) * generator.Parameters.CellSize,
                        0f, (path[index].y + .5f) * generator.Parameters.CellSize));
                    position.y = spawn.y;
                    yield return Touch(player, position);
                    Vector3 wallPosition = position;
                    foreach (var direction in new[] { Vector3.right, Vector3.left, Vector3.forward, Vector3.back })
                        if (Physics.Raycast(position + Vector3.up, direction, out var wall, 2f, ~0, QueryTriggerInteraction.Ignore) &&
                            wall.collider.GetComponentInParent<CharacterController>() == null)
                        {
                            wallPosition += direction * Mathf.Max(0f, wall.distance - player.radius - .08f);
                            break;
                        }
                    foreach (var testPosition in new[] { position, wallPosition })
                    {
                        yield return Touch(player, testPosition);
                        foreach (float elevation in new[] { 0f, 35f })
                            for (int angle = 0; angle < 360; angle += 45)
                            {
                                yaw.SetValue(motor, (float)angle);
                                pitch.SetValue(motor, elevation);
                                yield return new WaitForSeconds(.2f);
                                AssertCameraClear("cell " + path[index] + ", yaw " + angle + ", pitch " + elevation);
                            }
                    }
                }
                var pause = Object.FindFirstObjectByType<GameplayPauseController>();
                pause.Pause();
                Vector3 pausedPosition = Camera.main.transform.position;
                yield return new WaitForSecondsRealtime(.2f);
                Assert.That(Vector3.Distance(Camera.main.transform.position, pausedPosition), Is.LessThan(.01f));
                pause.Resume();
                player.enabled = false;
                player.transform.position += Vector3.down * 12f;
                player.enabled = true;
                yield return new WaitForSeconds(.6f);
                Assert.That(Vector3.Distance(player.transform.position, spawn), Is.LessThan(.2f));
                Assert.That(Vector3.Distance(Camera.main.transform.position, player.transform.position), Is.LessThan(4f),
                    "The camera must follow the recovered player, not remain in the old corridor.");
                AssertCameraClear("after fall recovery");
            }
            finally
            {
                Time.timeScale = 1f;
                if (motor != null) type.GetField("LockCameraPosition").SetValue(motor, false);
                Object.FindFirstObjectByType<PatchDialoguePresenter>()?.Hide();
            }
        }

        private static void AssertCameraClear(string context)
        {
            var camera = Camera.main;
            for (int x = 0; x <= 2; x++)
                for (int y = 0; y <= 2; y++)
                {
                    Vector3 point = camera.ViewportToWorldPoint(new Vector3(x * .5f, y * .5f, camera.nearClipPlane));
                    var obstacles = Physics.OverlapSphere(point, .015f, ~0, QueryTriggerInteraction.Ignore)
                        .Where(c => c.GetComponentInParent<CharacterController>() == null && !c.CompareTag("Player"));
                    Assert.That(obstacles.Select(c => c.name), Is.Empty, "Camera near plane intersects geometry: " + context);
                }
        }

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
                var coins = Object.FindFirstObjectByType<CoinManager>();
                Assert.That(coins.TotalCoins, Is.EqualTo(40));
                Assert.That(coins.RequiredCoins, Is.EqualTo(30));
                Assert.That(Object.FindObjectsByType<Coin>(FindObjectsSortMode.None).Length, Is.EqualTo(40));
                PatchDialoguePresenter.Ensure().Hide();

                var move = inputs.GetType().GetField("move");
                var look = inputs.GetType().GetField("look");
                motor.GetType().GetField("LockCameraPosition").SetValue(motor, true);
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
                    Vector3 heading = target - player.transform.position;
                    motor.GetType().GetField("_cinemachineTargetYaw", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                        .SetValue(motor, Mathf.Atan2(heading.x, heading.z) * Mathf.Rad2Deg);
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
                    AssertCameraClear("route cell " + path[i]);
                }
                move.SetValue(inputs, Vector2.zero);
                Assert.That(coins.CollectedCoins, Is.GreaterThanOrEqualTo(30), "Actual route traversal must collect enough coins.");
                Assert.That(GameObject.Find("CompletionMessage").GetComponent<Text>().text, Does.Contain("PHISHING MAZE COMPLETE"));
                yield return new WaitForSecondsRealtime(6.2f);
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

        [UnityTest]
        public IEnumerator Level2_PhysicalCoinPickupsBlockExitAt29Allow30AndResetOnReload()
        {
            SceneManager.LoadScene("Game_Level02");
            yield return null;
            yield return null;
            try
            {
                var player = GameObject.FindGameObjectWithTag("Player").GetComponent<CharacterController>();
                var motor = player.GetComponent("StarterAssets.ThirdPersonController") as Behaviour;
                motor.enabled = false;
                var manager = Object.FindFirstObjectByType<CoinManager>();
                var coins = Object.FindObjectsByType<Coin>(FindObjectsSortMode.None);
                Assert.That(coins.Length, Is.EqualTo(40));
                Assert.That(coins.Select(c => c.transform.position).Distinct().Count(), Is.EqualTo(40));
                foreach (var coin in coins)
                {
                    Assert.That(coin.GetComponent<SphereCollider>().isTrigger, Is.True);
                    Assert.That(coin.GetComponent<Rigidbody>().isKinematic, Is.True);
                    Assert.That(Physics.CheckSphere(coin.transform.position, .75f, ~0, QueryTriggerInteraction.Ignore), Is.False,
                        "Coin must not intersect a wall or floor.");
                    Assert.That(Physics.Raycast(coin.transform.position, Vector3.down, 2f, ~0, QueryTriggerInteraction.Ignore), Is.True,
                        "Coin must have ground below it.");
                }
                var exit = Object.FindFirstObjectByType<ModuleCompletionTrigger>().transform.position;
                float feetY = player.transform.position.y;
                var hud = GameObject.Find("CoinCounter").GetComponent<TMPro.TMP_Text>();
                Assert.That(hud.text, Does.Contain("EXIT LOCKED"));
                var rotation = coins[0].transform.rotation;
                yield return new WaitForSeconds(.1f);
                Assert.That(Quaternion.Angle(rotation, coins[0].transform.rotation), Is.GreaterThan(1f));
                yield return Touch(player, new Vector3(exit.x, feetY, exit.z));
                yield return new WaitForSecondsRealtime(2.2f);
                Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("Game_Level02"));
                Assert.That(GameObject.Find("CompletionMessage"), Is.Null);
                PatchDialoguePresenter.Ensure().Hide();
                for (int i = 0; i < 29; i++)
                {
                    var position = coins[i].transform.position;
                    yield return Touch(player, new Vector3(position.x, feetY, position.z));
                    Assert.That(manager.CollectedCoins, Is.EqualTo(i + 1), "Pickup " + i);
                    var feedback = GameObject.Find("CoinPickupFeedback").GetComponent<TMPro.TMP_Text>();
                    Assert.That(feedback.text, Is.EqualTo("+1 COIN"));
                    Assert.That(feedback.alpha, Is.GreaterThan(0f));
                }
                manager.CollectCoin(null);
                manager.CollectCoin(coins[0]);
                Assert.That(manager.CollectedCoins, Is.EqualTo(29), "Null/repeated pickups must not add coins.");
                yield return Touch(player, new Vector3(exit.x, feetY, exit.z));
                yield return new WaitForSecondsRealtime(2.2f);
                Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("Game_Level02"));
                Assert.That(manager.CanExit, Is.False);
                Assert.That(GameObject.Find("CompletionMessage"), Is.Null);
                PatchDialoguePresenter.Ensure().Hide();
                var last = coins[29].transform.position;
                yield return Touch(player, new Vector3(last.x, feetY, last.z));
                Assert.That(manager.CollectedCoins, Is.EqualTo(30));
                Assert.That(manager.CanExit, Is.True);
                Assert.That(hud.text, Does.Contain("EXIT UNLOCKED"));
                Assert.That(GameObject.Find("ExitSign").GetComponent<TMPro.TMP_Text>().text, Does.Contain("EXIT UNLOCKED"));
                Assert.That(GameObject.Find("CoinPickupFeedback").GetComponent<TMPro.TMP_Text>().text, Is.EqualTo("EXIT UNLOCKED"));
                Assert.That(PatchDialoguePresenter.Ensure().IsShowing, Is.True);
                yield return Touch(player, new Vector3(exit.x, feetY, exit.z));
                yield return new WaitForSecondsRealtime(6.2f);
                Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("LevelSelect"));
                SceneManager.LoadScene("Game_Level02");
                yield return null;
                yield return null;
                Assert.That(Object.FindFirstObjectByType<CoinManager>().CollectedCoins, Is.Zero);
                Assert.That(Object.FindObjectsByType<Coin>(FindObjectsSortMode.None).Length, Is.EqualTo(40));
            }
            finally
            {
                Time.timeScale = 1f;
                Object.FindFirstObjectByType<PatchDialoguePresenter>()?.Hide();
            }
        }

        [UnityTest]
        public IEnumerator Level2_MenuPresentationReusesLevelOneSkyMusicAndRespectsSfxMute()
        {
            const string sfxKey = "CyberRakshak.Sfx";
            bool hadVolume = PlayerPrefs.HasKey(sfxKey);
            float savedVolume = PlayerPrefs.GetFloat(sfxKey, 1f);
            try
            {
                SceneManager.LoadScene("Game_Level01");
                yield return null;
                var sky = RenderSettings.skybox;
                var music = Object.FindFirstObjectByType<SceneMusicPlayer>().GetComponent<AudioSource>().clip;
                var levelOnePanel = GameObject.Find("HealthPanel").GetComponent<Image>();
                var statusColor = levelOnePanel.color;
                Vector2 statusMin = levelOnePanel.rectTransform.anchorMin, statusMax = levelOnePanel.rectTransform.anchorMax;
                var levelOneObjective = GameObject.Find("ObjectiveLabel").GetComponent<Text>();
                var objectiveColor = levelOneObjective.color;
                int objectiveSize = levelOneObjective.fontSize;
                var objectiveFont = levelOneObjective.font;
                SceneManager.LoadScene("LevelSelect");
                yield return null;
                Assert.That(GameObject.Find("LevelTwoTitle").GetComponent<Text>().text, Is.EqualTo("Phishing Maze"));
                GameObject.Find("LevelTwoHit").GetComponent<Button>().onClick.Invoke();
                yield return null;
                yield return null;
                Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("Game_Level02"));
                Assert.That(RenderSettings.skybox, Is.EqualTo(sky));
                Assert.That(Camera.main.clearFlags, Is.EqualTo(CameraClearFlags.Skybox));
                var mazePanel = GameObject.Find("CoinPanel").GetComponent<Image>();
                Assert.That(mazePanel.color, Is.EqualTo(statusColor));
                Assert.That(mazePanel.rectTransform.anchorMin, Is.EqualTo(statusMin));
                Assert.That(mazePanel.rectTransform.anchorMax, Is.EqualTo(statusMax));
                var mazeObjective = GameObject.Find("MazeObjective").GetComponent<Text>();
                Assert.That(mazeObjective.transform.parent.name, Is.EqualTo("ObjectivePanel"));
                Assert.That(mazeObjective.color, Is.EqualTo(objectiveColor));
                Assert.That(mazeObjective.fontSize, Is.EqualTo(objectiveSize));
                Assert.That(mazeObjective.font, Is.EqualTo(objectiveFont));
                Assert.That(mazeObjective.fontStyle, Is.EqualTo(FontStyle.Bold));
                Assert.That(mazeObjective.text, Does.StartWith("OBJECTIVE:"));
                var counter = GameObject.Find("CoinCounter").GetComponent<TMPro.TMP_Text>();
                Assert.That(counter.color, Is.EqualTo(Color.white));
                Assert.That(counter.fontStyle, Is.EqualTo(TMPro.FontStyles.Bold));
                counter.ForceMeshUpdate();
                Assert.That(counter.isTextOverflowing, Is.False);
                Assert.That(counter.raycastTarget, Is.False);
                Assert.That(GameObject.Find("MazeModuleTitle").GetComponent<Text>().text, Is.EqualTo("PHISHING MAZE"));
                var controls = GameObject.Find("MazeControls").GetComponent<Text>();
                Assert.That(controls.preferredWidth, Is.LessThanOrEqualTo(controls.rectTransform.rect.width));
                Assert.That(GameObject.Find("MazeControlsPanel").GetComponent<Image>().raycastTarget, Is.False);
                var source = Object.FindFirstObjectByType<SceneMusicPlayer>().GetComponent<AudioSource>();
                Assert.That(source.clip, Is.EqualTo(music));
                Assert.That(source.isPlaying, Is.True);
                Assert.That(PatchDialoguePresenter.Ensure().IsShowing, Is.True);
                Assert.That(Resources.Load<PatchDialogueLine>("Dialogue/PhishingMazeIntro").Text, Does.Contain("30 of the 40"));
                Assert.That(GameObject.Find("PhishingMaze_Start"), Is.Not.Null);
                Assert.That(GameObject.Find("PhishingMaze_Finish"), Is.Not.Null);
                Assert.That(GameObject.Find("PhishingMaze_Start").GetComponentsInChildren<Collider>().Length, Is.Zero);
                Assert.That(GameObject.Find("PhishingMaze_Finish").GetComponentsInChildren<Collider>().Length, Is.Zero);
                foreach (string markerName in new[] { "PhishingMaze_Start", "PhishingMaze_Finish" })
                {
                    var marker = GameObject.Find(markerName).transform;
                    var floor = Physics.RaycastAll(marker.position + Vector3.up * 10f, Vector3.down, 30f, ~0,
                        QueryTriggerInteraction.Ignore).Where(h => h.collider.GetComponentInParent<CharacterController>() == null &&
                            !h.collider.CompareTag("Player") && !h.collider.transform.root.CompareTag("Player"))
                        .OrderBy(h => h.distance).First();
                    Assert.That(marker.position.y - floor.point.y, Is.EqualTo(.02f).Within(.01f), "Marker must sit on the floor.");
                }
                PatchDialoguePresenter.Ensure().Hide();
                PlayerPrefs.SetFloat(sfxKey, 0f);
                CyberRakshak.Platformer.PlatformerSfx.PlayCoinChime();
                CyberRakshak.Platformer.PlatformerSfx.PlayCoinChime(true);
                Assert.That(GameObject.Find("CoinPickupSfx"), Is.Null);
                Assert.That(GameObject.Find("MazeUnlockedSfx"), Is.Null);
                PlayerPrefs.SetFloat(sfxKey, .5f);
                CyberRakshak.Platformer.PlatformerSfx.PlayCoinChime();
                var pickup = GameObject.Find("CoinPickupSfx").GetComponent<AudioSource>();
                Assert.That(pickup.isPlaying, Is.True);
                Assert.That(pickup.volume, Is.EqualTo(.275f).Within(.001f));
                var samples = new float[pickup.clip.samples];
                pickup.clip.GetData(samples, 0);
                Assert.That(samples.Any(s => Mathf.Abs(s) > .01f), Is.True);
                CyberRakshak.Platformer.PlatformerSfx.PlayCoinChime(true);
                Assert.That(GameObject.Find("MazeUnlockedSfx").GetComponent<AudioSource>().isPlaying, Is.True);
                yield return new WaitForSeconds(.5f);
                Assert.That(GameObject.Find("CoinPickupSfx"), Is.Null);
                Assert.That(GameObject.Find("MazeUnlockedSfx"), Is.Null);
            }
            finally
            {
                if (hadVolume) PlayerPrefs.SetFloat(sfxKey, savedVolume); else PlayerPrefs.DeleteKey(sfxKey);
                Time.timeScale = 1f;
                Object.FindFirstObjectByType<PatchDialoguePresenter>()?.Hide();
            }
        }

        private static IEnumerator Touch(CharacterController player, Vector3 position)
        {
            player.enabled = false;
            player.transform.position = position;
            player.enabled = true;
            Physics.SyncTransforms();
            player.Move(Vector3.down * .01f);
            yield return new WaitForFixedUpdate();
            yield return null;
        }
    }
}
