using System.Reflection;
using UnityEngine;

namespace CyberRakshak.Platformer
{
    /// <summary>Bridges authored platformer objects to Starter Assets without adding that assembly as a dependency.</summary>
    public static class PlatformerMotionAdapter
    {
        private const BindingFlags PublicInstance = BindingFlags.Instance | BindingFlags.Public;

        public static void BeginTraversalOverride(CharacterController controller)
        {
            Invoke(controller, "BeginTraversalOverride");
        }

        public static void MoveTraversalOverride(CharacterController controller, Vector3 delta)
        {
            if (!Invoke(controller, "MoveTraversalOverride", delta))
            {
                controller.Move(delta);
            }
        }

        public static void EndTraversalOverride(CharacterController controller)
        {
            Invoke(controller, "EndTraversalOverride", -2f);
        }

        public static bool IsTraversalOverrideActive(CharacterController controller)
        {
            Component motor = FindMotor(controller);
            PropertyInfo property = motor?.GetType().GetProperty("IsTraversalOverrideActive", PublicInstance);
            return property != null && property.PropertyType == typeof(bool) && (bool)property.GetValue(motor);
        }

        public static void ApplyEnvironmentalDisplacement(CharacterController controller, Vector3 delta)
        {
            if (!Invoke(controller, "ApplyEnvironmentalDisplacement", delta))
            {
                controller.Move(delta);
            }
        }

        private static bool Invoke(CharacterController controller, string methodName, params object[] arguments)
        {
            Component motor = FindMotor(controller);
            MethodInfo method = motor?.GetType().GetMethod(methodName, PublicInstance);
            if (method == null)
            {
                return false;
            }

            method.Invoke(motor, arguments);
            return true;
        }

        private static Component FindMotor(CharacterController controller)
        {
            return controller != null ? controller.GetComponent("ThirdPersonController") : null;
        }
    }
}
