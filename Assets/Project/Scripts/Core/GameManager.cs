using UnityEngine;
using UnityEngine.SceneManagement;
using GroceryGame.Data;
using GroceryGame.UI;

namespace GroceryGame.Core
{
    public class GameManager : MonoBehaviour
    {
        // Singleton pattern - allows global access
        private static GameManager _instance;
        public static GameManager Instance => _instance;

        // Current game state
        [SerializeField] private GameState _currentState = GameState.MainMenu;
        public GameState CurrentState => _currentState;

        // References to other managers (assigned in inspector)
        [Header("Manager References")]
        [SerializeField] private UIManager uiManager;
        [SerializeField] private RecipeManager recipeManager;
        [SerializeField] private StoreManager storeManager;

        // Current selected data
        private Recipe _selectedRecipe;
        private StoreLocation _selectedStore;

        // Public access to selected data
        public Recipe SelectedRecipe => _selectedRecipe;
        public StoreLocation SelectedStore => _selectedStore;

        private void Awake()
        {
            // Singleton setup
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            // Set target framerate
            Application.targetFrameRate = 60;
        }

        private void Start()
        {
            // Start at the main menu
            TransitionToState(GameState.MainMenu);
        }

        // Change the game state
        public void TransitionToState(GameState newState)
        {
            // Exit current state
            OnExitState(_currentState);

            // Update current state
            _currentState = newState;

            // Enter new state
            OnEnterState(_currentState);

            // Debug logging
            Debug.Log($"Game state changed to: {_currentState}");
        }

        // Clean up when exiting a state
        private void OnExitState(GameState state)
        {
            switch (state)
            {
                case GameState.MainMenu:
                    break;

                case GameState.RecipeSelection:
                    if (uiManager != null)
                        uiManager.HideRecipeSelection();
                    break;

                case GameState.StoreSelection:
                    if (uiManager != null)
                        uiManager.HideStoreSelection();
                    break;

                case GameState.Shopping:
                    // Shopping cleanup (will implement later)
                    break;

                    // Add other states as needed
            }
        }

        // Set up when entering a state
        private void OnEnterState(GameState state)
        {
            switch (state)
            {
                case GameState.MainMenu:
                    if (uiManager != null)
                        uiManager.ShowMainMenu();
                    break;

                case GameState.RecipeSelection:
                    if (uiManager != null && recipeManager != null)
                        uiManager.ShowRecipeSelection(recipeManager.GetAvailableRecipes());
                    break;

                case GameState.StoreSelection:
                    if (uiManager != null && storeManager != null)
                        uiManager.ShowStoreSelection(storeManager.GetAvailableStores());
                    break;

                case GameState.Shopping:
                    StartShopping();
                    break;

                case GameState.RecipeSuccess:
                    if (uiManager != null)
                        uiManager.ShowRecipeSuccess(_selectedRecipe);
                    break;

                case GameState.RecipeFailed:
                    if (uiManager != null)
                        uiManager.ShowRecipeFailed(_selectedRecipe);
                    break;
            }
        }

        // Select a recipe then show store selection
        public void SelectRecipe(Recipe recipe)
        {
            _selectedRecipe = recipe;
            TransitionToState(GameState.StoreSelection);
        }

        // Select a store and start shopping
        public void SelectStore(StoreLocation store)
        {
            _selectedStore = store;
            TransitionToState(GameState.Shopping);
        }

        // Start a shopping trip
        private void StartShopping()
        {
            // Safety checks
            if (_selectedRecipe == null)
            {
                Debug.LogError("Attempted to start shopping without a selected recipe!");
                TransitionToState(GameState.RecipeSelection);
                return;
            }

            if (_selectedStore == null)
            {
                Debug.LogError("Attempted to start shopping without a selected store!");
                TransitionToState(GameState.StoreSelection);
                return;
            }

            // Load the appropriate store scene
            SceneManager.LoadScene(_selectedStore.sceneName);
        }

        // Handle scene loading completion
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // If we're in shopping state and scene loaded matches our selected store
            if (_currentState == GameState.Shopping && scene.name == _selectedStore.sceneName)
            {
                // Initialize shopping (will implement later)
                Debug.Log($"Loaded store: {_selectedStore.storeName} for recipe: {_selectedRecipe.recipeName}");
            }
        }

        private void OnEnable()
        {
            // Subscribe to scene loaded event
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            // Unsubscribe from events
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}