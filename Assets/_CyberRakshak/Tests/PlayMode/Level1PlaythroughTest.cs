using System.Collections;
using CyberRakshak.Platformer;
using CyberRakshak.PATCH;
using TMPro;
using CyberRakshak.Runtime;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

namespace CyberRakshak.Tests
{
    public sealed class Level1PlaythroughTest
    {
        [UnityTearDown]
        public IEnumerator RestoreTimeAfterTest()
        {
            Object.FindFirstObjectByType<PatchDialoguePresenter>()?.Hide();
            Time.timeScale = 1f;
            yield return null;
        }

        [UnityTest]
        public IEnumerator Settings_VisibleBackButtonClosesInBothGameplayScenes()
        {
            foreach (string scene in new[] { "Game_Tutorial", "Game_Level01" })
            {
                SceneManager.LoadScene("Assets/_CyberRakshak/Scenes/" + scene + ".unity");
                yield return null;
                var pause = Object.FindFirstObjectByType<GameplayPauseController>();
                Assert.That(pause, Is.Not.Null);
                pause.Pause();
                pause.OpenSettings();
                yield return null;
                Canvas.ForceUpdateCanvases();

                var back = GameObject.Find("SettingsBackHit").GetComponent<Button>();
                var artwork = (RectTransform)back.transform.parent;
                // Centre of BACK in the approved 1366 x 768 settings artwork.
                Vector3 worldPoint = artwork.TransformPoint(new Vector3(
                    artwork.rect.xMin + artwork.rect.width * .503f,
                    artwork.rect.yMin + artwork.rect.height * .21f));
                var pointer = new PointerEventData(EventSystem.current)
                {
                    position = RectTransformUtility.WorldToScreenPoint(null, worldPoint)
                };
                var hits = new List<RaycastResult>();
                EventSystem.current.RaycastAll(pointer, hits);
                Assert.That(hits.Count, Is.GreaterThan(0), scene);
                Assert.That(hits[0].gameObject.GetComponentInParent<Button>(), Is.EqualTo(back),
                    scene + ": the visible Back artwork must hit the Back button, not decoration.");
                ExecuteEvents.Execute(back.gameObject, pointer, ExecuteEvents.pointerClickHandler);
                yield return null;
                Assert.That(back.gameObject.activeInHierarchy, Is.False, scene);
                Assert.That(Time.timeScale, Is.EqualTo(0f), "Back must return to pause, not resume gameplay.");
                pause.Resume();
            }
        }

        [UnityTest]
        public IEnumerator LevelSelect_NativeCardsShowProgressAndRouteAllAvailableButtons()
        {
            string[] keys = { "CyberRakshak.TutorialComplete", "CyberRakshak.Level1Complete" };
            int[] saved = keys.Select(k => PlayerPrefs.GetInt(k, -1)).ToArray();
            try
            {
                string[] buttons = { "TutorialHit", "LevelOneHit", "LevelTwoHit", "BackHit" };
                string[] destinations = { "Game_Tutorial", "Game_Level01", "Game_Level02", "MainMenu" };
                for (int route = 0; route < buttons.Length; route++)
                {
                    // Menu entry must release state inherited from gameplay or an open pause menu.
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                    Time.timeScale = 0f;
                    SceneManager.LoadScene("Assets/_CyberRakshak/Scenes/LevelSelect.unity");
                    yield return null;
                    Assert.That(Cursor.lockState, Is.EqualTo(CursorLockMode.None));
                    Assert.That(Cursor.visible, Is.True);
                    Assert.That(Time.timeScale, Is.EqualTo(1f));
                    var controller = Object.FindFirstObjectByType<LevelSelectController>();
                    Assert.That(controller, Is.Not.Null);
                    var tutorial = GameObject.Find("TutorialHit").GetComponent<Button>();
                    var levelOne = GameObject.Find("LevelOneHit").GetComponent<Button>();
                    var levelTwo = GameObject.Find("LevelTwoHit").GetComponent<Button>();
                    Assert.That(tutorial.interactable && levelOne.interactable, Is.True);
                    Assert.That(levelTwo.interactable, Is.True);
                    Assert.That(GameObject.Find("LevelOneTitle").GetComponent<Text>().text,
                        Is.EqualTo("Firewall Foundations"));
                    Assert.That(GameObject.Find("Background").GetComponent<Image>().sprite.name,
                        Is.EqualTo("MainMenu_Background_1920x1080"), "No baked locks or labels in the backdrop.");
                    foreach (int complete in new[] { 0, 1 })
                    {
                        foreach (string key in keys) PlayerPrefs.SetInt(key, complete);
                        controller.Refresh();
                        foreach (string label in new[] { "TutorialStatus", "LevelOneStatus" })
                        {
                            var text = GameObject.Find(label).GetComponent<Text>();
                            Assert.That(text.text, Is.EqualTo(complete == 1 ? "REPLAY" : "PLAY"));
                            Assert.That(text.raycastTarget, Is.False);
                        }
                        Assert.That(GameObject.Find("LevelTwoStatus").GetComponent<Text>().text,
                            Is.EqualTo("PLAY"));
                    }
                    Assert.That(tutorial.FindSelectableOnDown(), Is.EqualTo(levelOne));
                    Assert.That(levelOne.FindSelectableOnDown(), Is.EqualTo(levelTwo));
                    Assert.That(levelTwo.FindSelectableOnDown().name, Is.EqualTo("BackHit"));
                    Canvas.ForceUpdateCanvases();
                    // Raycast depth is assigned by the first rendered frame after scene load.
                    yield return null;
                    var button = GameObject.Find(buttons[route]).GetComponent<Button>();
                    var rect = (RectTransform)button.transform;
                    var pointer = new PointerEventData(EventSystem.current)
                    {
                        position = RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center)),
                        button = PointerEventData.InputButton.Left
                    };
                    var hits = new List<RaycastResult>();
                    EventSystem.current.RaycastAll(pointer, hits);
                    Assert.That(hits.Count, Is.GreaterThan(0), buttons[route] + " at " + pointer.position +
                        "; graphic depth=" + button.image.depth);
                    Assert.That(hits[0].gameObject.GetComponentInParent<Button>(), Is.EqualTo(button),
                        "Decoration must not intercept the visible card.");
                    ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerClickHandler);
                    yield return null;
                    Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(destinations[route]));
                }
            }
            finally
            {
                for (int i = 0; i < keys.Length; i++)
                    if (saved[i] < 0) PlayerPrefs.DeleteKey(keys[i]);
                    else PlayerPrefs.SetInt(keys[i], saved[i]);
                PlayerPrefs.Save();
            }
        }

        [UnityTest]
        public IEnumerator Level1_LeverActivationClearsItsBlockingFirewall()
        {
            SceneManager.LoadScene("Assets/_CyberRakshak/Scenes/Game_Level01.unity");
            yield return null;
            var lever = Object.FindFirstObjectByType<LeverSwitch>();
            Assert.That(lever, Is.Not.Null);
            Assert.That(lever.firewall, Is.Not.Null);
            var blocker = lever.firewall.GetComponent<Collider>();
            Assert.That(blocker.enabled, Is.True);
            Assert.That(lever.waterfall.activeSelf, Is.False);
            // Controlled activation tests the sequence, not E-key reachability or its visual quality.
            lever.SendMessage("ActivateLever", SendMessageOptions.RequireReceiver);
            Assert.That(lever.waterfall.activeSelf, Is.True);
            yield return new WaitForSeconds(lever.firewall.extinguishDelay + .2f);
            Assert.That(blocker.enabled, Is.False);
            Assert.That(Object.FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None)
                .Any(p => p.name == "FirewallCore" || p.name == "FirewallCrest"), Is.False,
                "Runtime fire layers must be inactive after extinguishing.");
        }

        [UnityTest]
        public IEnumerator Level1_HasRequiredPlayerRouteWiring()
        {
            SceneManager.LoadScene("Assets/_CyberRakshak/Scenes/Game_Level01.unity");
            yield return null;
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            GameObject gate = GameObject.Find("Firewall_Gate");
            LeverSwitch lever = Object.FindFirstObjectByType<LeverSwitch>();
            GameObject completionZone = GameObject.Find("Level1_Complete_Zone");

            Assert.That(player, Is.Not.Null, "Level 1 needs a tagged player.");
            Assert.That(player.GetComponent<PlayerHealth>(), Is.Not.Null, "Level 1 player needs health/restart handling.");
            Assert.That(Object.FindFirstObjectByType<PlatformerHud>(), Is.Not.Null, "Level 1 needs its health HUD.");
            Assert.That(Object.FindObjectsByType<PlatformerEnemy>(FindObjectsSortMode.None).Length, Is.EqualTo(4));
            Assert.That(gate?.GetComponent<Collider>(), Is.Not.Null, "Firewall_Gate needs a collider.");
            Assert.That(lever?.waterfall, Is.Not.Null, "Water lever needs its waterfall reference.");
            Assert.That(lever?.firewall, Is.Not.Null, "Water lever needs the exact firewall reference.");
            Assert.That(completionZone, Is.Not.Null, "Level 1 needs a completion trigger after the treadmill section.");
            Assert.That(completionZone?.GetComponent<BoxCollider>(), Is.Not.Null, "Level 1 completion zone needs a collider.");
            Assert.That(completionZone != null && completionZone.GetComponent<BoxCollider>().isTrigger, Is.True, "Level 1 completion collider must be a trigger.");
        }

        [UnityTest]
        public IEnumerator Level1_AllFourAuthoredEnemiesAcceptDescendingControllerStomps()
        {
            SceneManager.LoadScene("Assets/_CyberRakshak/Scenes/Game_Level01.unity");
            yield return null;
            var controller = Object.FindFirstObjectByType<CharacterController>();
            // Isolate collision response from player steering; use real scene enemies and collision callbacks.
            ((Behaviour)controller.GetComponent("StarterAssets.ThirdPersonController")).enabled = false;
            var health = controller.GetComponent<PlayerHealth>();
            var enemies = Object.FindObjectsByType<PlatformerEnemy>(FindObjectsSortMode.None);
            Assert.That(enemies.Length, Is.EqualTo(4));
            Assert.That(Object.FindFirstObjectByType<EnemySectionBarrier>(), Is.Not.Null,
                "The patrol encounter needs its clearance barrier.");
            Assert.That(GameObject.Find("ObjectiveLabel").GetComponent<Text>().text, Is.EqualTo("OBJECTIVE: CLEAR THE PATROL"));
            foreach (var enemy in enemies)
                enemy.enabled = false;

            foreach (var enemy in enemies)
            {
                Bounds bounds = enemy.GetComponent<Collider>().bounds;
                PlaceFeet(controller, new Vector3(bounds.center.x, bounds.max.y + .15f, bounds.center.z));
                for (int step = 0; step < 12 && !enemy.IsDefeated; step++)
                {
                    controller.Move(Vector3.down * .1f);
                    yield return null;
                }
                Assert.That(enemy.IsDefeated, Is.True, enemy.name);
                Assert.That(health.CurrentHealth, Is.EqualTo(100), "A descending stomp must not damage the player.");
                if (PlayerPrefs.GetFloat("CyberRakshak.Sfx", 1f) > 0f)
                {
                    var audio = GameObject.Find("EnemyBlopSfx")?.GetComponent<AudioSource>();
                    Assert.That(audio, Is.Not.Null, "Stomp must create its blop emitter.");
                    Assert.That(audio.clip.samples, Is.GreaterThan(0));
                    Assert.That(audio.volume, Is.GreaterThan(0f));
                    Assert.That(audio.isPlaying, Is.True, "Defeat must start playback, not just create an emitter.");
                    var samples = new float[audio.clip.samples];
                    audio.clip.GetData(samples, 0);
                    Assert.That(samples.Max(v => Mathf.Abs(v)), Is.GreaterThan(.2f), "Blop must have an audible waveform.");
                }
                yield return new WaitForSeconds(.4f);
                Assert.That(enemy == null, Is.True, "Defeated enemy must be removed.");
            }

            yield return null;
            Assert.That(Object.FindFirstObjectByType<EnemySectionBarrier>(), Is.Null,
                "The route barrier must clear after the final patrol enemy is defeated.");
            Assert.That(GameObject.Find("ObjectiveLabel").GetComponent<Text>().text,
                Is.EqualTo("OBJECTIVE: USE THE LAUNCH PAD TO REACH WATER CONTROL"));
        }

        [UnityTest]
        public IEnumerator Level1_AuthoredPadsLaunchAndReachTheirTargets()
        {
            SceneManager.LoadScene("Assets/_CyberRakshak/Scenes/Game_Level01.unity");
            yield return null;
            var controller = GameObject.FindGameObjectWithTag("Player").GetComponent<CharacterController>();
            var pads = Object.FindObjectsByType<JumpBoosterPad>(FindObjectsSortMode.None);
            Assert.That(pads.Length, Is.EqualTo(1));
            foreach (var pad in pads) pad.enabled = false;

            foreach (var pad in pads)
            {
                Assert.That(pad.launchTargetName, Is.Not.Null.And.Not.Empty, pad.name + " has no launch destination.");
                var target = GameObject.Find(pad.launchTargetName);
                Assert.That(target, Is.Not.Null, pad.name + " target is missing.");
                var colliders = pad.GetComponentsInChildren<Collider>().Where(c => c.enabled && !c.isTrigger).ToArray();
                Assert.That(colliders.Length, Is.GreaterThan(0));
                Bounds bounds = colliders[0].bounds;
                foreach (var collider in colliders.Skip(1)) bounds.Encapsulate(collider.bounds);
                PlaceFeet(controller, new Vector3(bounds.center.x, bounds.max.y + .05f, bounds.center.z));
                controller.Move(Vector3.down * .1f);
                pad.enabled = true;
                bool launched = false;
                float highest = controller.transform.position.y;
                for (float elapsed = 0f; elapsed < pad.bounceDuration + 1f; elapsed += Time.deltaTime)
                {
                    launched |= PlatformerMotionAdapter.IsTraversalOverrideActive(controller);
                    highest = Mathf.Max(highest, controller.transform.position.y);
                    yield return null;
                }
                Assert.That(launched, Is.True, pad.name + " did not auto-launch a grounded player.");
                Assert.That(highest, Is.GreaterThan(bounds.max.y + 1f), pad.name + " did not rise.");
                Assert.That(Vector3.Distance(Vector3.ProjectOnPlane(controller.transform.position, Vector3.up),
                    Vector3.ProjectOnPlane(target.transform.position, Vector3.up)), Is.LessThan(3f),
                    pad.name + " did not reach its target horizontally.");
                Assert.That(controller.isGrounded, Is.True, pad.name + " did not finish with a stable landing.");
                pad.enabled = false;
            }
        }

        [UnityTest]
        public IEnumerator Level1_HealthBarWidthMatchesItsLabel()
        {
            SceneManager.LoadScene("Assets/_CyberRakshak/Scenes/Game_Level01.unity");
            yield return null;
            var hud = Object.FindFirstObjectByType<PlatformerHud>();
            var fill = GameObject.Find("HealthBarFill").GetComponent<Image>();
            var background = (RectTransform)fill.transform.parent;
            foreach (int health in new[] { 100, 66, 32, 0 })
            {
                hud.SetHealth(health, 100);
                Canvas.ForceUpdateCanvases();
                Assert.That(fill.rectTransform.rect.width / background.rect.width,
                    Is.EqualTo(health / 100f).Within(.001f), "Rendered width at " + health + " HP");
                Assert.That(GameObject.Find("HealthLabel").GetComponent<Text>().text,
                    Is.EqualTo("SYSTEM INTEGRITY  " + health + "/100"));
            }
            hud.SetHealth(1, 0);
            Assert.That(fill.rectTransform.anchorMax.x, Is.EqualTo(0f), "Invalid maximum must not corrupt the rect.");
        }

        [UnityTest]
        public IEnumerator Level1_TreadmillsCarryGroundedPlayer()
        {
            SceneManager.LoadScene("Assets/_CyberRakshak/Scenes/Game_Level01.unity");
            yield return null;
            var controller = Object.FindFirstObjectByType<CharacterController>();
            ((Behaviour)controller.GetComponent("StarterAssets.ThirdPersonController")).enabled = false;
            var belts = Object.FindObjectsByType<TreadmillPlatform>(FindObjectsSortMode.None);
            var presenter = PatchDialoguePresenter.Ensure();
            presenter.Hide();
            bool firstBelt = true;
            Assert.That(belts.Length, Is.GreaterThan(0));
            foreach (var belt in belts)
            {
                Bounds bounds = belt.GetComponent<Collider>().bounds;
                var markers = belt.GetComponentsInChildren<LineRenderer>();
                Assert.That(markers.Length, Is.EqualTo(3), "Each belt needs direction cues.");
                Vector3 markerStart = markers[0].GetPosition(1);
                Assert.That(Vector3.Dot(markers[0].GetPosition(1) -
                    (markers[0].GetPosition(0) + markers[0].GetPosition(2)) * .5f,
                    belt.backwardDirection.normalized), Is.GreaterThan(0f), "Chevron must point with the belt push.");
                PlaceFeet(controller, new Vector3(bounds.center.x, bounds.max.y + .05f, bounds.center.z));
                Vector3 start = controller.transform.position;
                for (float elapsed = 0f; elapsed < 1f; elapsed += Time.deltaTime)
                {
                    controller.Move(Vector3.down * .1f);
                    yield return null;
                }
                Assert.That(Vector3.Dot(controller.transform.position - start, belt.backwardDirection.normalized),
                    Is.GreaterThan(.2f), belt.name + " did not carry the player.");
                Assert.That(controller.bounds.center.x, Is.InRange(bounds.min.x, bounds.max.x));
                Assert.That(controller.bounds.center.z, Is.InRange(bounds.min.z, bounds.max.z));
                Assert.That(controller.isGrounded, Is.True, belt.name);
                Assert.That(Vector3.Distance(markers[0].GetPosition(1), markerStart), Is.GreaterThan(.2f),
                    "Direction cues must scroll rather than look like a static floor.");
                Assert.That(presenter.IsShowing, Is.EqualTo(firstBelt), "Treadmill instruction must appear only on the first belt.");
                presenter.Hide();
                firstBelt = false;
            }
            Assert.That(presenter.ShowOnce("level1-treadmill", "PATCH", "Duplicate"), Is.False,
                "Revisiting a belt must not repeat its instruction.");
        }

        [UnityTest]
        public IEnumerator Level1_FirewallBurnsTenPerSecondAndStopsOutsideOrAfterWater()
        {
            SceneManager.LoadScene("Assets/_CyberRakshak/Scenes/Game_Level01.unity");
            yield return null;
            PatchDialoguePresenter.Ensure().Hide();
            var controller = GameObject.FindGameObjectWithTag("Player").GetComponent<CharacterController>();
            ((Behaviour)controller.GetComponent("StarterAssets.ThirdPersonController")).enabled = false;
            var health = controller.GetComponent<PlayerHealth>();
            var firewall = Object.FindFirstObjectByType<LeverSwitch>().firewall;
            Bounds gate = firewall.GetComponent<Collider>().bounds;
            Vector3 contact = new Vector3(gate.center.x, .05f, gate.min.z - controller.radius - .02f);
            Assert.That(firewall.damagePerSecond, Is.EqualTo(10f));
            PlaceFeet(controller, contact);
            yield return null;
            Assert.That(health.TakeHit(1), Is.True);
            int startHealth = health.CurrentHealth;
            float startTime = Time.time;
            yield return new WaitForSeconds(1.05f);
            Assert.That(startHealth - health.CurrentHealth,
                Is.EqualTo(10f * (Time.time - startTime)).Within(1.1f),
                "Burn must ignore the enemy-contact invulnerability timer and integrate at 10 HP/sec.");

            Time.timeScale = 0f;
            int pausedHealth = health.CurrentHealth;
            yield return new WaitForSecondsRealtime(.3f);
            Assert.That(health.CurrentHealth, Is.EqualTo(pausedHealth), "Paused gameplay must not burn.");
            Time.timeScale = 1f;
            PlaceFeet(controller, contact + Vector3.back * 3f);
            yield return null;
            int safeHealth = health.CurrentHealth;
            yield return new WaitForSeconds(.3f);
            Assert.That(health.CurrentHealth, Is.EqualTo(safeHealth), "Leaving the gate must stop burn damage.");

            PlaceFeet(controller, contact);
            firewall.BeginExtinguish();
            firewall.BeginExtinguish(); // Duplicate water events must not start duplicate sequences.
            yield return new WaitForSeconds(firewall.extinguishDelay + .2f);
            int extinguishedHealth = health.CurrentHealth;
            yield return new WaitForSeconds(.3f);
            Assert.That(health.CurrentHealth, Is.EqualTo(extinguishedHealth), "Extinguished fire must not burn.");
            Assert.That(firewall.GetComponent<Collider>().enabled, Is.False);

            health.TakeBurnDamage(health.CurrentHealth + 1f);
            yield return null;
            var respawned = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerHealth>();
            Assert.That(respawned.CurrentHealth, Is.EqualTo(100), "Lethal burn must restart with full integrity.");
            Assert.That(respawned.transform.position.z, Is.LessThan(gate.min.z - 10f));
        }

        [UnityTest]
        public IEnumerator Dialogue_PatchChatterFollowsRevealAndRespectsMute()
        {
            const string key = "CyberRakshak.Sfx";
            bool hadVolume = PlayerPrefs.HasKey(key);
            float savedVolume = PlayerPrefs.GetFloat(key, 1f);
            try
            {
                SceneManager.LoadScene("Assets/_CyberRakshak/Scenes/Game_Tutorial.unity");
                yield return null;
                var presenter = PatchDialoguePresenter.Ensure();
                presenter.Hide();
                PlayerPrefs.SetFloat(key, .7f);
                presenter.ShowSequence(new PatchDialoguePresenter.DialogueMessage("PATCH",
                    "Computerized chatter should follow this line while letters appear, even when gameplay time is stopped.", .1f));
                var voice = presenter.GetComponent<AudioSource>();
                Assert.That(voice, Is.Not.Null);
                bool heardChatter = false;
                float voiceDeadline = Time.realtimeSinceStartup + 1.2f;
                while (Time.realtimeSinceStartup < voiceDeadline)
                {
                    heardChatter |= voice.isPlaying;
                    yield return null;
                }
                Assert.That(Time.timeScale, Is.EqualTo(0f));
                Assert.That(heardChatter, Is.True, "PATCH voice must run during an input-blocking dialogue; clip=" +
                    (voice.clip != null ? voice.clip.name : "none") + "; listenerPaused=" + AudioListener.pause);
                Assert.That(voice.clip.name, Is.EqualTo("PatchChatter"));
                Assert.That(voice.volume, Is.EqualTo(.7f * .35f).Within(.001f));
                presenter.Advance();
                Assert.That(voice.isPlaying, Is.False, "Reveal-all must stop chatter immediately.");
                presenter.Hide();

                PlayerPrefs.SetFloat(key, 0f);
                presenter.Show("PATCH", "This line must be silent while SFX are muted.");
                yield return new WaitForSecondsRealtime(.5f);
                Assert.That(voice.isPlaying, Is.False);
                presenter.Hide();
                PlayerPrefs.SetFloat(key, 1f);
                presenter.Show("JAY", "Human speakers must not use PATCH's robot chatter.");
                yield return new WaitForSecondsRealtime(.5f);
                Assert.That(voice.isPlaying, Is.False);
                presenter.Hide();
            }
            finally
            {
                if (hadVolume) PlayerPrefs.SetFloat(key, savedVolume);
                else PlayerPrefs.DeleteKey(key);
            }
        }

        private static void PlaceFeet(CharacterController controller, Vector3 feet)
        {
            PlatformerMotionAdapter.BeginTraversalOverride(controller);
            PlatformerMotionAdapter.EndTraversalOverride(controller);
            float offset = controller.bounds.min.y - controller.transform.position.y;
            controller.enabled = false;
            controller.transform.position = feet - Vector3.up * offset;
            controller.enabled = true;
            Physics.SyncTransforms();
        }

        [UnityTest]
        public IEnumerator Level1_FinalRouteCanBeCrossedWithThePlayerMotor()
        {
            SceneManager.LoadScene("Assets/_CyberRakshak/Scenes/Game_Level01.unity");
            yield return null;
            var controller = GameObject.FindGameObjectWithTag("Player").GetComponentInChildren<CharacterController>();
            var motor = controller.GetComponent("StarterAssets.ThirdPersonController");
            var inputs = controller.GetComponent("StarterAssetsInputs") ?? controller.GetComponent("StarterAssets.StarterAssetsInputs");
            Assert.That(inputs, Is.Not.Null);
            var move = inputs.GetType().GetField("move");
            var jump = inputs.GetType().GetField("jump");
            // Keep camera-relative steering facing down the authored route.
            var cameraTarget = (GameObject)motor.GetType().GetField("CinemachineCameraTarget").GetValue(motor);
            cameraTarget.transform.rotation = Quaternion.identity;
            motor.GetType().GetField("_cinemachineTargetYaw", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(motor, 0f);
            string[] route = { "TrainingGround (6)", "Cube_Regular_Yellow", "Cube_Regular_Purple1", "Cube_Regular_Yellow (6)",
                "Cube_Regular_Purple2", "Cube_Regular_Yellow (4)", "Cube_Regular_Purple3", "Cube_Regular_Green" };
            var surfaces = route.Select(name => GameObject.Find(name).GetComponent<Collider>()).ToArray();
            Bounds first = surfaces[0].bounds;
            PlaceFeet(controller, new Vector3(-2f, first.max.y + .05f, first.max.z - .8f));
            move.SetValue(inputs, Vector2.zero);
            jump.SetValue(inputs, false);
            yield return new WaitForSeconds(.4f);

            for (int index = 1; index < surfaces.Length; index++)
            {
                Bounds previous = surfaces[index - 1].bounds;
                Bounds destination = surfaces[index].bounds;
                move.SetValue(inputs, Vector2.up);
                float deadline = Time.time + 8f;
                while (controller.transform.position.z < previous.max.z - .8f && Time.time < deadline)
                    yield return null;
                Assert.That(controller.isGrounded, Is.True, route[index - 1] + " approach");
                jump.SetValue(inputs, true);
                yield return null;
                jump.SetValue(inputs, false);
                yield return new WaitForSeconds(1.05f);
                move.SetValue(inputs, Vector2.zero);
                yield return new WaitForSeconds(.2f);
                Assert.That(controller.bounds.center.z, Is.InRange(destination.min.z, destination.max.z), route[index]);
                Assert.That(controller.isGrounded, Is.True, route[index] + " landing");
            }

            Bounds finish = surfaces[surfaces.Length - 1].bounds;
            Bounds exit = GameObject.Find("Level1_Complete_Zone").GetComponent<BoxCollider>().bounds;
            Assert.That(exit.center.z, Is.InRange(finish.min.z, finish.max.z), "Finish trigger must be over a solid landing surface.");
        }

        [UnityTest]
        public IEnumerator Tutorial_HasFallRecoveryAndCompletionZone()
        {
            SceneManager.LoadScene("Assets/_CyberRakshak/Scenes/Game_Tutorial.unity");
            yield return null;

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            GameObject completionZone = GameObject.Find("Tutorial_Complete_Zone");
            Assert.That(player?.GetComponentInChildren<SceneFallRecovery>(), Is.Not.Null, "Tutorial player needs fall recovery.");
            Assert.That(completionZone, Is.Not.Null, "Tutorial needs a completion trigger.");
            Assert.That(completionZone?.GetComponent<BoxCollider>(), Is.Not.Null, "Tutorial completion zone needs a collider.");
            Assert.That(completionZone != null && completionZone.GetComponent<BoxCollider>().isTrigger, Is.True, "Tutorial completion collider must be a trigger.");
        }

        [UnityTest]
        public IEnumerator Tutorial_OnboardingPortraitsCompleteIntoPlayableTutorial()
        {
            SceneManager.LoadScene("Assets/_CyberRakshak/Scenes/Game_Tutorial.unity");
            yield return null;
            var presenter = PatchDialoguePresenter.Ensure();
            var stage = presenter.transform.Find("NarrativeStage").gameObject;
            var jay = stage.transform.Find("JayPortrait").GetComponent<Image>();
            var adi = stage.transform.Find("AdiPortrait").GetComponent<Image>();
            var player = GameObject.FindGameObjectWithTag("Player");
            var input = player.GetComponent("PlayerInput");
            var inputActive = input.GetType().GetProperty("inputIsActive");
            var body = presenter.GetComponentsInChildren<TMP_Text>().Single(t => t.name == "Message");
            var speaker = presenter.GetComponentsInChildren<TMP_Text>().Single(t => t.name == "Speaker");
            Assert.That(stage.activeSelf, Is.True);
            Assert.That(jay.sprite.name, Is.EqualTo("JayPortrait"));
            Assert.That(adi.sprite.name, Is.EqualTo("AdiPortrait"));
            Assert.That(stage.transform.Find("OfficeBackdrop").GetComponent<Image>().sprite, Is.Not.Null);
            Assert.That(jay.rectTransform.anchorMin.x, Is.LessThan(adi.rectTransform.anchorMin.x));
            Assert.That((bool)inputActive.GetValue(input), Is.False);
            Vector3 start = player.transform.position;
            foreach (string expected in new[] { "JAY", "ADI", "PATCH" })
            {
                yield return new WaitForSecondsRealtime(.3f);
                Assert.That(speaker.text, Is.EqualTo(expected));
                Assert.That(body.isTextOverflowing, Is.False, expected);
                Assert.That(Time.timeScale, Is.EqualTo(0f));
                presenter.Advance();
                Assert.That(body.maxVisibleCharacters, Is.EqualTo(body.textInfo.characterCount));
                if (expected == "JAY")
                {
                    Canvas.ForceUpdateCanvases();
                    yield return null;
                    var button = presenter.GetComponentInChildren<Button>();
                    var rect = (RectTransform)button.transform;
                    var pointer = new PointerEventData(EventSystem.current)
                    {
                        position = RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center)),
                        button = PointerEventData.InputButton.Left
                    };
                    var hits = new List<RaycastResult>();
                    EventSystem.current.RaycastAll(pointer, hits);
                    Assert.That(hits.First().gameObject.GetComponentInParent<Button>(), Is.EqualTo(button),
                        "Office backdrop must not intercept the dialogue button.");
                    ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerClickHandler);
                }
                else presenter.Advance();
                yield return new WaitForSecondsRealtime(.45f);
            }
            Assert.That(presenter.IsShowing, Is.False);
            Assert.That(stage.activeSelf, Is.False);
            Assert.That(Time.timeScale, Is.EqualTo(1f));
            Assert.That((bool)inputActive.GetValue(input), Is.True);
            Assert.That(Vector3.Distance(player.transform.position, start), Is.LessThan(.3f));
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("Game_Tutorial"));
        }

        [UnityTest]
        public IEnumerator Dialogue_RevealThenAdvanceAndPauseRestoreGameplay()
        {
            SceneManager.LoadScene("Assets/_CyberRakshak/Scenes/Game_Tutorial.unity");
            yield return null;
            var presenter = PatchDialoguePresenter.Ensure();
            presenter.Hide();
            const string first = "A firewall checks traffic against rules. Allowed traffic passes; blocked traffic stays out.";
            const string second = "Keep checking the route ahead.";
            presenter.ShowSequence(new PatchDialoguePresenter.DialogueMessage("PATCH", first, .1f),
                new PatchDialoguePresenter.DialogueMessage("PATCH", second, .1f));
            yield return new WaitForSecondsRealtime(.4f);
            var body = presenter.GetComponentsInChildren<TMP_Text>().Single(t => t.name == "Message");
            Assert.That(Time.timeScale, Is.EqualTo(0f));
            Assert.That(Cursor.lockState, Is.EqualTo(CursorLockMode.None));
            Assert.That(body.maxVisibleCharacters, Is.LessThan(body.textInfo.characterCount));
            presenter.Advance();
            Assert.That(body.maxVisibleCharacters, Is.EqualTo(body.textInfo.characterCount));
            yield return new WaitForSecondsRealtime(.35f);
            Assert.That(body.text, Is.EqualTo(first), "Reveal must not also advance or expire the line.");
            Canvas.ForceUpdateCanvases();
            yield return null;
            var button = presenter.GetComponentInChildren<Button>();
            var rect = (RectTransform)button.transform;
            var pointer = new PointerEventData(EventSystem.current)
            {
                position = RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center)),
                button = PointerEventData.InputButton.Left
            };
            var hits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, hits);
            Assert.That(hits.First().gameObject.GetComponentInParent<Button>(), Is.EqualTo(button));
            ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerClickHandler);
            yield return new WaitForSecondsRealtime(.5f);
            Assert.That(body.text, Is.EqualTo(second));
            var pause = Object.FindFirstObjectByType<GameplayPauseController>();
            pause.Pause();
            presenter.Advance();
            pause.Resume();
            Assert.That(Time.timeScale, Is.EqualTo(0f), "Closing pause must not release an active story.");
            presenter.Show("PATCH", "A gameplay hint must not replace a story.", .1f);
            Assert.That(body.text, Is.EqualTo(second));
            presenter.Advance();
            presenter.Advance();
            yield return new WaitForSecondsRealtime(.35f);
            Assert.That(presenter.IsShowing, Is.False);
            Assert.That(Time.timeScale, Is.EqualTo(1f));
            presenter.Show("PATCH", "A short gameplay hint.", .1f);
            Assert.That(Time.timeScale, Is.EqualTo(1f), "Gameplay hints must remain non-blocking.");
            presenter.Hide();
        }

        [UnityTest]
        public IEnumerator Modules_RecoverFromFallsAndCompleteThroughPhysicalTriggers()
        {
            int tutorialProgress = PlayerPrefs.GetInt("CyberRakshak.TutorialComplete", 0);
            int levelProgress = PlayerPrefs.GetInt("CyberRakshak.Level1Complete", 0);
            string latestScene = GameProgression.LatestUnlockedScene;
            try
            {
                foreach (string scene in new[] { "Game_Tutorial", "Game_Level01" })
                {
                    SceneManager.LoadScene("Assets/_CyberRakshak/Scenes/" + scene + ".unity");
                    yield return null;
                    // This test exercises physical traversal; story input is verified separately.
                    PatchDialoguePresenter.Ensure().Hide();
                    var controller = GameObject.FindGameObjectWithTag("Player").GetComponentInChildren<CharacterController>();
                    Vector3 spawn = controller.transform.position;
                    PlaceFeet(controller, new Vector3(spawn.x, -10f, spawn.z));
                    yield return new WaitForSeconds(.5f);
                    controller = GameObject.FindGameObjectWithTag("Player").GetComponentInChildren<CharacterController>();
                    Assert.That(Vector3.Distance(controller.transform.position, spawn), Is.LessThan(1f), scene + " fall recovery");

                    if (scene == "Game_Level01")
                    {
                        var health = controller.GetComponent<PlayerHealth>();
                        for (int hit = 0; hit < 3; hit++)
                        {
                            health.TakeHit(34);
                            yield return new WaitForSeconds(.85f);
                        }
                        controller = GameObject.FindGameObjectWithTag("Player").GetComponentInChildren<CharacterController>();
                        Assert.That(controller.GetComponent<PlayerHealth>().CurrentHealth, Is.EqualTo(100), "Death must restore full health.");
                        Assert.That(Vector3.Distance(controller.transform.position, spawn), Is.LessThan(1f), "Death must return to the level start.");
                    }

                    var exit = GameObject.Find(scene == "Game_Tutorial" ? "Tutorial_Complete_Zone" : "Level1_Complete_Zone").GetComponent<BoxCollider>();
                    ((Behaviour)controller.GetComponent("StarterAssets.ThirdPersonController")).enabled = false;
                    Bounds bounds = exit.bounds;
                    PlaceFeet(controller, new Vector3(bounds.center.x, bounds.min.y + .05f, bounds.min.z - 1f));
                    for (int step = 0; step < 20; step++)
                    {
                        controller.Move(Vector3.forward * .1f + Vector3.down * .02f);
                        yield return null;
                    }
                    yield return new WaitForSecondsRealtime(2.2f);
                    Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("LevelSelect"), scene + " physical exit routing");
                    Assert.That(Cursor.lockState, Is.EqualTo(CursorLockMode.None), scene + " menu cursor must be free");
                    Assert.That(Cursor.visible, Is.True, scene + " menu cursor must be visible");
                    Assert.That(scene == "Game_Tutorial" ? GameProgression.HasCompletedTutorial : GameProgression.HasCompletedLevelOne, Is.True);
                }
            }
            finally
            {
                PlayerPrefs.SetInt("CyberRakshak.TutorialComplete", tutorialProgress);
                PlayerPrefs.SetInt("CyberRakshak.Level1Complete", levelProgress);
                PlayerPrefs.SetString("CyberRakshak.LatestUnlockedScene", latestScene);
                PlayerPrefs.Save();
            }
        }
    }
}
