using System.Collections.Generic;
using UnityEngine;

namespace SelectablePlus.Navigation {

    public interface ISelectableNavigationBuilder {
        void BuildNavigation(SelectableGroup group);
    }

    /// <summary>
    /// Build navigation data using the same method as Unity does for it's own UI navigation.
    /// Generally the recommended option. This advanced version lets you specify a minimum score and a maximum distance for each option to pass.
    /// Parameters set to a negative value will be skipped!
    /// </summary>
    public class AdvancedUnityNavigationBuilder : ISelectableNavigationBuilder {
        private float maxDistance;
        private float minScore;

        public AdvancedUnityNavigationBuilder(float maxDistance = -1, float minScore = -1) {
            this.maxDistance = maxDistance;
            this.minScore = minScore;
        }

        public void BuildNavigation(SelectableGroup group) {
            Vector3 position;
            Vector3 direction;
            Vector3 localDir;
            float maxScore;

            foreach (SelectableOptionBase option in group.options) {
                Transform transform = option.GetTransform();

                for (int i = 0; i < option.navigationArray.Length; i++) {
                    maxScore = Mathf.NegativeInfinity;

                    direction = SelectableNavigationUtils.GetDirectionVector((SelectableNavigationDirection)i);
                    localDir = Quaternion.Inverse(transform.rotation) * direction;
                    position = transform.TransformPoint(SelectableNavigationUtils.GetPointOnRectEdge(transform as RectTransform, localDir));

                    SelectableOptionBase bestPick = null;
                    foreach (SelectableOptionBase other in group.options) {
                        SelectableOptionBase sel = other;

                        if (sel == option || sel == null)
                            continue;

                        var selRect = sel.GetTransform() as RectTransform;
                        Vector3 selCenter = selRect != null ? (Vector3)selRect.rect.center : Vector3.zero;
                        Vector3 myVector = sel.GetTransform().TransformPoint(selCenter) - position;

                        float dot = Vector3.Dot(direction, myVector);

                        // Skip elements that are in the wrong direction or which have zero or too much distance.
                        // This also ensures that the scoring formula below will not have a division by zero error.
                        if (dot <= 0 || (maxDistance > 0 && myVector.magnitude > maxDistance))
                            continue;

                        // This scoring function has two priorities:
                        // - Score higher for positions that are closer.
                        // - Score higher for positions that are located in the right direction.
                        // This scoring function combines both of these criteria.
                        float score = dot / myVector.sqrMagnitude;

                        // Skip elements whose score is lower than the minimum score.
                        if (minScore > 0 && score < minScore)
                            continue;

                        if (score > maxScore) {
                            maxScore = score;
                            bestPick = sel;
                        }

                    }

                    option.navigationArray[i] = bestPick;
                }
            }


        }
    }

    /// <summary>
    /// Build navigation data using the same method as Unity does for it's own UI navigation.
    /// Generally the recommended option.
    /// </summary>
    public class UnityNavigationBuilder : ISelectableNavigationBuilder {
        public void BuildNavigation(SelectableGroup group) {
            Vector3 position;
            Vector3 direction;
            Vector3 localDir;
            float maxScore;

            foreach (SelectableOptionBase option in group.options) {
                Transform transform = option.GetTransform();

                for (int i = 0; i < option.navigationArray.Length; i++) {
                    maxScore = Mathf.NegativeInfinity;

                    direction = SelectableNavigationUtils.GetDirectionVector((SelectableNavigationDirection)i);
                    localDir = Quaternion.Inverse(transform.rotation) * direction;
                    position = transform.TransformPoint(SelectableNavigationUtils.GetPointOnRectEdge(transform as RectTransform, localDir));

                    SelectableOptionBase bestPick = null;
                    foreach (SelectableOptionBase other in group.options) {
                        SelectableOptionBase sel = other;

                        if (sel == option || sel == null)
                            continue;

                        var selRect = sel.GetTransform() as RectTransform;
                        Vector3 selCenter = selRect != null ? (Vector3)selRect.rect.center : Vector3.zero;
                        Vector3 myVector = sel.GetTransform().TransformPoint(selCenter) - position;

                        // Value that is the distance out along the direction.
                        float dot = Vector3.Dot(direction, myVector);

                        // Skip elements that are in the wrong direction or which have zero or too much distance.
                        // This also ensures that the scoring formula below will not have a division by zero error.
                        if (dot <= 0)
                            continue;

                        // This scoring function has two priorities:
                        // - Score higher for positions that are closer.
                        // - Score higher for positions that are located in the right direction.
                        // This scoring function combines both of these criteria.
                        float score = dot / myVector.sqrMagnitude;

                        if (score > maxScore) {
                            maxScore = score;
                            bestPick = sel;
                        }
                    }

                    option.navigationArray[i] = bestPick;
                }
            }


        }
    }

    /// <summary>
    /// Build navigation data using raycasts in all 4 directions from the center of the RectTransform of every option.
    /// Slow and messy, but as accurate as possible. Best used for baked navigation.
    /// </summary>
    public class RaycastNavigationBuilder : ISelectableNavigationBuilder {
        float[] maxSearchDistances;

        public RaycastNavigationBuilder(float[] maxSearchDistances) {
            this.maxSearchDistances = maxSearchDistances;
        }

        public void BuildNavigation(SelectableGroup group) {
            BuildSmartNavigation(SelectableNavigationUtils.SortByYPosFirst(group.options), maxSearchDistances);
        }

        private static void BuildSmartNavigation(List<SelectableOptionBase> options, float[] maxSearchDistances) {
            List<BoxCollider2D> cachedColliders = new List<BoxCollider2D>();
            List<RectTransform> cachedTransforms = new List<RectTransform>();
            List<int> cachedLayerIndices = new List<int>();

            //Attach BoxCollider2Ds to every option and set to to default layer to allow Raycasts to work
            foreach (SelectableOptionBase option in options) {
                BoxCollider2D collider = option.gameObject.AddComponent<BoxCollider2D>();
                RectTransform optionRectTransform = option.GetComponent<RectTransform>();

                collider.size = optionRectTransform.sizeDelta;
                cachedColliders.Add(collider);
                cachedTransforms.Add(optionRectTransform);
                cachedLayerIndices.Add(option.gameObject.layer);

                option.gameObject.layer = 0;
            }

            //Raycast from each control border to a direction for the given distance for the direction
            for (int i = 0; i < options.Count; i++) {
                for (int j = 0; j < maxSearchDistances.Length; j++) {
                    SelectableNavigationDirection direction = (SelectableNavigationDirection)j;
                    RectTransform optionRectTransform = cachedTransforms[i];

                    RaycastHit2D hit = Physics2D.Raycast(GetRaycastOrigin(direction, optionRectTransform), SelectableNavigationUtils.GetDirectionVector(direction), maxSearchDistances[j]);

                    if (hit.transform != null) {
                        SelectableOptionBase hitOption = hit.transform.GetComponent<SelectableOptionBase>();
                        if (hitOption != null) {
                            options[i].SetNextOption(direction, hitOption);
                        }
                    }
                }
            }

            //Destroy the previously created BoxColliders and restore previous layer
            for (int i = 0; i < options.Count; i++) {
                if (Application.isPlaying) {
                    Object.Destroy(cachedColliders[i]);
                } else {
                    Object.DestroyImmediate(cachedColliders[i]);
                }
                options[i].gameObject.layer = cachedLayerIndices[i];
            }
        }

        private static Vector2 GetRaycastOrigin(SelectableNavigationDirection raycastDirection, RectTransform optionTransform) {
            Vector2 center = optionTransform.position;
            switch (raycastDirection) {
                case SelectableNavigationDirection.UP:
                    return center + new Vector2(0, optionTransform.sizeDelta.y / 2 + 1);
                case SelectableNavigationDirection.RIGHT:
                    return center + new Vector2(optionTransform.sizeDelta.x / 2 + 1, 0);
                case SelectableNavigationDirection.DOWN:
                    return center - new Vector2(0, optionTransform.sizeDelta.y / 2 + 1);
                case SelectableNavigationDirection.LEFT:
                    return center - new Vector2(optionTransform.sizeDelta.x / 2 + 1, 0);
            }

            return center;
        }
    }

    /// <summary>
    /// Build navigation data using x or y coordinate order.
    /// Very fast, so this is safe to use at runtime.
    /// </summary>
    public class AxisNavigationBuilder : ISelectableNavigationBuilder {

        public enum SORTING_AXIS { X, Y };
        public SORTING_AXIS axis;

        public AxisNavigationBuilder(SORTING_AXIS axis) {
            this.axis = axis;
        }

        public void BuildNavigation(SelectableGroup group) {
            List<SelectableOptionBase> sortedOptions;
            if (axis == SORTING_AXIS.X) {
                sortedOptions = SelectableNavigationUtils.SortByXPosFirst(group.options);
            } else if (axis == SORTING_AXIS.Y) {
                sortedOptions = SelectableNavigationUtils.SortByYPosFirst(group.options);
            } else {
                //Use as a fallback if no axis is set
                sortedOptions = group.options;
            }

            SelectableNavigationUtils.BuildNavigationFromSortedList(sortedOptions, group.navigationType);
        }
    }

    /// <summary>
    /// Build navigation data using a sorted list of SelectableOptionBase objects,
    /// in the direction set in the group settings.
    /// </summary>
    public class SortedListNavigationBuilder : ISelectableNavigationBuilder {
        public void BuildNavigation(SelectableGroup group) {
            SelectableNavigationUtils.BuildNavigationFromSortedList(group.options, group.navigationType);
        }
    }

    public class SelectableNavigationUtils {

        public static void BuildNavigationFromSortedList(List<SelectableOptionBase> sortedOptions, SelectableGroup.NavigationBuildType direction) {
            switch (direction) {
                case SelectableGroup.NavigationBuildType.HORIZONTAL:
                    sortedOptions[0].SetNextOption(SelectableNavigationDirection.RIGHT, sortedOptions[1]);
                    for (int i = 1; i <= sortedOptions.Count - 2; i++) {
                        sortedOptions[i].SetNextOption(SelectableNavigationDirection.LEFT, sortedOptions[i - 1]);
                        sortedOptions[i].SetNextOption(SelectableNavigationDirection.RIGHT, sortedOptions[i + 1]);
                    }
                    sortedOptions[sortedOptions.Count - 1].SetNextOption(SelectableNavigationDirection.LEFT, sortedOptions[sortedOptions.Count - 2]);
                    break;

                case SelectableGroup.NavigationBuildType.VERTICAL:
                    sortedOptions[0].SetNextOption(SelectableNavigationDirection.DOWN, sortedOptions[1]);
                    for (int i = 1; i <= sortedOptions.Count - 2; i++) {
                        sortedOptions[i].SetNextOption(SelectableNavigationDirection.UP, sortedOptions[i - 1]);
                        sortedOptions[i].SetNextOption(SelectableNavigationDirection.DOWN, sortedOptions[i + 1]);
                    }
                    sortedOptions[sortedOptions.Count - 1].SetNextOption(SelectableNavigationDirection.UP, sortedOptions[sortedOptions.Count - 2]);
                    break;
            }
        }

        public static List<SelectableOptionBase> SortByXPosFirst(List<SelectableOptionBase> list) {
            list.Sort(delegate (SelectableOptionBase b, SelectableOptionBase a) {
                Transform aTransform = a.GetComponent<Transform>();
                Transform bTransform = b.GetComponent<Transform>();

                int x = aTransform.position.x.CompareTo(bTransform.position.x);

                if (x == 0)
                    return bTransform.position.y.CompareTo(aTransform.position.y);

                return x;
            });

            return list;
        }

        public static List<SelectableOptionBase> SortByYPosFirst(List<SelectableOptionBase> list) {
            list.Sort(delegate (SelectableOptionBase b, SelectableOptionBase a) {
                Transform aTransform = a.GetComponent<Transform>();
                Transform bTransform = b.GetComponent<Transform>();

                int x = aTransform.position.y.CompareTo(bTransform.position.y);

                if (x == 0)
                    return bTransform.position.x.CompareTo(aTransform.position.x);

                return x;
            });

            return list;
        }

        public static Vector2 GetDirectionVector(SelectableNavigationDirection direction) {
            switch (direction) {
                case SelectableNavigationDirection.UP:
                    return new Vector2(0, 1);
                case SelectableNavigationDirection.RIGHT:
                    return new Vector2(1, 0);
                case SelectableNavigationDirection.DOWN:
                    return new Vector2(0, -1);
                case SelectableNavigationDirection.LEFT:
                    return new Vector2(-1, 0);
            }

            return Vector2.zero;
        }

        public static Vector3 GetPointOnRectEdge(RectTransform rect, Vector2 direction) {
            if (rect == null)
                return Vector3.zero;
            if (direction != Vector2.zero)
                direction /= Mathf.Max(Mathf.Abs(direction.x), Mathf.Abs(direction.y));
            direction = rect.rect.center + Vector2.Scale(rect.rect.size, direction * 0.5f);
            return direction;
        }
    }

}


