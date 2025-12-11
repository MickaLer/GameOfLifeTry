using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace GameOfLife.Script
{
    public class GameOfLife : MonoBehaviour
    {
        [SerializeField] private Text sessionsText;
        [SerializeField] private Text rowsText;
        [SerializeField] private Text columnsText;
        [SerializeField] private Text sliderText;
        [SerializeField] private Slider timeSlider;

        [SerializeField] private HorizontalLayoutGroup layoutGroup;
        [SerializeField] private VerticalLayoutGroup verticalPrefab;
        [SerializeField] private GameObject button;

        private readonly List<Status[,]> _list = new();
        private bool _active = true;
        private bool _actual;
        private int _columns = 1;
        private bool _future;
        private int _numberOfSessions;
        private bool _pressed;

        private int _rows = 1;
        private float _timer = 1;

        private int Rows
        {
            get => _rows;
            set
            {
                if (value <= 0) return;

                _rows = value;
                Debug.Log(_rows);
                LayoutGroupManager();
            }
        }

        private int Columns
        {
            get => _columns;
            set
            {
                if (value <= 0) return;

                _columns = value;
                Debug.Log(_columns);
                LayoutGroupManager();
            }
        }

        private void Update()
        {
            rowsText.text = _rows.ToString();
            columnsText.text = _columns.ToString();
            _timer = timeSlider.value;
            sessionsText.text = _numberOfSessions.ToString();
        }

        public void Pause()
        {
            if (_active)
            {
                _active = false;
                _actual = false;
                StopAllCoroutines();
            }
            else
            {
                _active = true;
                _actual = true;
                StartNoAleatory();
            }
        }

        private static void SetButton(GameObject variable)
        {
            variable.GetComponent<Image>().color =
                variable.GetComponent<Image>().color == Color.white ? Color.black : Color.white;
        }

        public void StartNoAleatory()
        {
            StopAllCoroutines();
            if (!_actual)
            {
                _numberOfSessions = 0;
                _list.Clear();
            }
            else
            {
                _actual = false;
            }

            _active = true;
            var grid = new Status[_rows, _columns];
            var verticalLayoutGroup = layoutGroup.GetComponentsInChildren<VerticalLayoutGroup>();
            var indexRow = 0;
            var indexColumn = 0;
            foreach (var t in verticalLayoutGroup)
            foreach (var variable in t.GetComponentsInChildren<Image>())
            {
                if (indexColumn >= _columns)
                {
                    indexColumn = 0;
                    indexRow += 1;
                }

                if (indexRow >= _rows) indexRow -= 1;

                if (variable.color == Color.white)
                    grid[indexRow, indexColumn] = Status.Dead;
                else
                    grid[indexRow, indexColumn] = Status.Alive;

                indexColumn += 1;
            }

            StartCoroutine(PrintRepeat(grid));
        }

        public void AleatoryGrid()
        {
            StopAllCoroutines();
            _active = true;
            _numberOfSessions = 0;
            var grid = new Status[_rows, _columns];
            for (var row = 0; row < _rows; row++)
            for (var column = 0; column < _columns; column++)
                grid[row, column] = (Status)RandomNumberGenerator.GetInt32(0, 2);

            _list.Clear();
            StartCoroutine(PrintRepeat(grid));
        }

        public void ChangeValueRows(int value)
        {
            if (_pressed)
            {
                _active = false;
                Rows += value;
            }
        }

        public void ChangeValueColumns(int value)
        {
            if (_pressed)
            {
                _active = false;
                Columns += value;
            }
        }

        private Status[,] NextGen(Status[,] currentGrid)
        {
            var nextGen = new Status[_rows, _columns];
            for (var row = 0; row < _rows; row++)
            for (var column = 0; column < _columns; column++)
            {
                var aliveNeighbors = 0;
                for (var i = -1; i < 2; i++)
                for (var j = -1; j < 2; j++)
                {
                    if (row + i < 0 || column + j < 0 || row + i >= _rows || column + j >= _columns ||
                        (i == 0 && j == 0)) continue;
                    aliveNeighbors += currentGrid[row + i, column + j] == Status.Alive ? 1 : 0;
                }

                var currentCell = currentGrid[row, column];
                if (currentCell == Status.Alive && aliveNeighbors < 2)
                    nextGen[row, column] = Status.Dead;
                else if (currentCell == Status.Alive && aliveNeighbors > 3)
                    nextGen[row, column] = Status.Dead;
                else if (currentCell == Status.Dead && aliveNeighbors == 3)
                    nextGen[row, column] = Status.Alive;
                else
                    nextGen[row, column] = currentCell;
            }

            _list.Add(currentGrid);
            return nextGen;
        }

        private IEnumerator PrintRepeat(Status[,] grid)
        {
            while (_active)
            {
                Print(grid);
                _numberOfSessions += 1;
                grid = NextGen(grid);
                if (_future)
                {
                    _active = false;
                    Print(grid);
                }

                if (_timer > Time.deltaTime)
                {
                    sliderText.text = (timeSlider.value - Time.deltaTime).ToString("0.000");
                    yield return new WaitForSeconds(_timer - Time.deltaTime);
                }

                sliderText.text = (Time.deltaTime + timeSlider.value).ToString("0.000");
                yield return new WaitForSeconds(_timer + Time.deltaTime);
            }
        }

        private void Print(Status[,] future)
        {
            ButtonTransformation(future, layoutGroup.GetComponentsInChildren<VerticalLayoutGroup>());
        }

        private void ButtonTransformation([CanBeNull] Status[,] future, VerticalLayoutGroup[] verticalLayoutGroups)
        {
            var indexRow = 0;
            var indexColumn = 0;
            foreach (var t in verticalLayoutGroups)
            foreach (var variable in t.GetComponentsInChildren<Image>())
            {
                if (indexColumn >= _columns)
                {
                    indexColumn = 0;
                    indexRow += 1;
                }

                if (indexRow >= _rows) indexRow -= 1;

                if (future != null && future[indexRow, indexColumn] == Status.Dead)
                    variable.color = Color.white;
                else
                    variable.color = Color.black;

                indexColumn += 1;
            }
        }


        private void LayoutGroupManager()
        {
            var verticalLayoutGroup = layoutGroup.GetComponentsInChildren<VerticalLayoutGroup>();
            if (verticalLayoutGroup.Length > Rows)
                for (var i = verticalLayoutGroup.Length; i > Rows; i--)
                    Destroy(verticalLayoutGroup[i - 1].gameObject);
            else
                for (var i = layoutGroup.transform.childCount; i < Rows; i++)
                    Instantiate(verticalPrefab, layoutGroup.transform);

            foreach (var t in verticalLayoutGroup)
            {
                var buttonLayout = t.GetComponentsInChildren<Button>();
                if (buttonLayout.Length > Columns)
                    for (var j = buttonLayout.Length; j > Columns; j--)
                        Destroy(buttonLayout[j - 1].gameObject);
                else
                    for (var j = buttonLayout.Length; j < Columns; j++)
                        Instantiate(button, t.transform);
            }

            foreach (var t in verticalLayoutGroup)
            foreach (var variable in t.GetComponentsInChildren<Button>())
            {
                variable.onClick.RemoveAllListeners();
                variable.onClick.AddListener(delegate { SetButton(variable.gameObject); });
            }
        }

        public void BackToBack(int value)
        {
            _active = false;
            _numberOfSessions += value;
            if (_list.Count <= _numberOfSessions)
            {
                _active = true;
                _actual = true;
                _future = true;
                _numberOfSessions -= value;
                StartNoAleatory();
                return;
            }

            _future = false;
            if (_numberOfSessions < 0)
            {
                _numberOfSessions = 0;
                return;
            }

            Print(_list[_numberOfSessions]);
        }

        public void ChangePressedValue(bool newValue)
        {
            _pressed = newValue;
        }

        public void ResetGrid()
        {
            _active = false;
            var verticalLayoutGroup = layoutGroup.GetComponentsInChildren<VerticalLayoutGroup>();
            var indexRow = 0;
            var indexColumn = 0;
            foreach (var t in verticalLayoutGroup)
            foreach (var variable in t.GetComponentsInChildren<Image>())
            {
                if (indexColumn >= Columns)
                {
                    indexColumn = 0;
                    indexRow += 1;
                }

                if (indexRow >= Rows) indexRow -= 1;

                variable.color = Color.white;
                indexColumn += 1;
            }

            _numberOfSessions = 0;
            _list.Clear();
        }

        public void GridPreset(int presetValue)
        {
            Rows = presetValue;
            Columns = presetValue;
            LayoutGroupManager();
        }

        private enum Status
        {
            Dead,
            Alive
        }
    }
}