// Copyright (c) Jeroen van Pienbroek. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using UnityEngine;
using System.Text;
using System;
using System.Collections.Generic;

namespace AdvancedInputFieldPlugin
{
	public class StandaloneKeyboard : NativeKeyboard
	{
		private TextValidator textValidator;
		private bool emojisAllowed;
		private bool secure;
		private LineType lineType;
		private int characterLimit;

		private string text;
		private int selectionStartPosition;
		private int selectionEndPosition;

		/// <summary>The text in the clipboard</summary>
		public static string Clipboard
		{
			get
			{
				return GUIUtility.systemCopyBuffer;
			}
			set
			{
				GUIUtility.systemCopyBuffer = value;
			}
		}

		public bool ShouldSubmit
		{
			get { return (lineType != LineType.MULTILINE_NEWLINE); }
		}

		public bool Multiline { get { return lineType != LineType.SINGLE_LINE; } }

		/// <summary>Indicates if some text is currently selected</summary>
		public bool HasSelection
		{
			get { return (selectionEndPosition > selectionStartPosition); }
		}

		private void Awake()
		{
			textValidator = new TextValidator();
			text = string.Empty;
			enabled = false;
		}

		public override void UpdateTextEdit(string text, int selectionStartPosition, int selectionEndPosition)
		{
			this.text = text;
			this.selectionStartPosition = selectionStartPosition;
			this.selectionEndPosition = selectionEndPosition;
		}

		public override void ShowKeyboard(string text, int selectionStartPosition, int selectionEndPosition, NativeKeyboardConfiguration configuration)
		{
			this.text = text;
			this.selectionStartPosition = selectionStartPosition;
			this.selectionEndPosition = selectionEndPosition;

			this.characterLimit = configuration.characterLimit;
			this.emojisAllowed = configuration.emojisAllowed;
			this.secure = configuration.secure;
			this.lineType = configuration.lineType;
			textValidator.Validation = configuration.characterValidation;
			textValidator.LineType = configuration.lineType;

			CharacterValidator characterValidator = null;
			if (!string.IsNullOrEmpty(configuration.characterValidatorJSON))
			{
				characterValidator = ScriptableObject.CreateInstance<CharacterValidator>();
				JsonUtility.FromJsonOverwrite(configuration.characterValidatorJSON, characterValidator);
			}
			textValidator.Validator = characterValidator;

			OnKeyboardShow();
		}

		public override void HideKeyboard()
		{
			OnKeyboardHide();
		}

		public override void EnableUpdates()
		{
			enabled = true;
			InputMethodManager.ClearEventQueue();
		}

		public override void DisableUpdates()
		{
			enabled = false;
		}

		private void Update()
		{
			Event keyboardEvent = new Event();
			while (Event.PopEvent(keyboardEvent))
			{
				if (keyboardEvent.rawType == EventType.KeyDown)
				{
					bool shouldContinue = ProcessKeyboardEvent(keyboardEvent);
					if (!shouldContinue)
					{
						return;
					}
				}

				if ((keyboardEvent.type == EventType.ValidateCommand || keyboardEvent.type == EventType.ExecuteCommand)
					&& keyboardEvent.commandName == "SelectAll")
				{
					SelectAll();
				}
			}

			InputEvent inputEvent;
			while (InputMethodManager.PopEvent(out inputEvent))
			{
				switch (inputEvent.Type)
				{
					case InputEventType.CHARACTER:
						CharacterInputEvent characterInputEvent = (CharacterInputEvent)inputEvent;
						TryInsertChar(characterInputEvent.character);
						break;
					case InputEventType.TEXT:
						TextInputEvent textInputEvent = (TextInputEvent)inputEvent;
						Insert(textInputEvent.text);
						break;
				}
			}
		}

		/// <summary>Processes a keyboard event</summary>
		/// <param name="keyboardEvent">The keyboard event to process</param>
		internal bool ProcessKeyboardEvent(Event keyboardEvent)
		{
			EventModifiers currentEventModifiers = keyboardEvent.modifiers;
			bool ctrl = SystemInfo.operatingSystemFamily == OperatingSystemFamily.MacOSX ? (currentEventModifiers & EventModifiers.Command) != 0 : (currentEventModifiers & EventModifiers.Control) != 0;
			bool shift = (currentEventModifiers & EventModifiers.Shift) != 0;
			bool alt = (currentEventModifiers & EventModifiers.Alt) != 0;
			bool ctrlOnly = ctrl && !alt && !shift;

			switch (keyboardEvent.keyCode)
			{
				case KeyCode.Backspace:
					OnSpecialKeyPressed(SpecialKeyCode.BACKSPACE);
					DeletePreviousChar();
					return true;
				case KeyCode.Delete:
					DeleteNextChar();
					return true;
				case KeyCode.Home:
					MoveToStart();
					return true;
				case KeyCode.End:
					MoveToEnd();
					return true;
				case KeyCode.A: //Select All
					if (ctrlOnly)
					{
						SelectAll();
						return true;
					}
					break;
				case KeyCode.C: //Copy
					if (ctrlOnly)
					{
						Copy();
						return true;
					}
					break;
				case KeyCode.V: //Paste
					if (ctrlOnly)
					{
						Paste();
						return true;
					}
					break;
				case KeyCode.X: //Cut
					if (ctrlOnly)
					{
						Cut();
						return true;
					}
					break;
				case KeyCode.LeftArrow:
					OnMoveLeft(shift, ctrl);
					return true;
				case KeyCode.RightArrow:
					OnMoveRight(shift, ctrl);
					return true;
				case KeyCode.DownArrow:
					OnMoveDown(shift, ctrl);
					return true;
				case KeyCode.UpArrow:
					OnMoveUp(shift, ctrl);
					return true;
				case KeyCode.Return: //Submit
				case KeyCode.KeypadEnter: //Submit
					if (ShouldSubmit)
					{
						OnKeyboardDone();
						return false;
					}
					break;
				case KeyCode.Escape:
					OnSpecialKeyPressed(SpecialKeyCode.ESCAPE);
					OnKeyboardCancel();
					return false;
				case KeyCode.Tab:
					OnKeyboardNext();
					return false;
			}

			char c = keyboardEvent.character;

			if (12593 <= c && 12643 >= c)
			{
				// IME 조합 문자열 처리
				if (!string.IsNullOrEmpty(Input.compositionString))
				{
					Insert(Input.compositionString);
					return true;
				}
			}

			if (!Multiline && (c == '\t' || c == '\r' || c == 10)) //Don't allow return chars or tabulator key to be entered into single line fields.
			{
				return true;
			}

			if (c == '\r' || (int)c == 3) //Convert carriage return and end-of-text characters to newline.
			{
				c = '\n';
			}

			TryInsertChar(c);

			return true;
		}

		/// <summary>Copies current text selection</summary>
		internal virtual void Copy()
		{
			if (!secure)
			{
				Clipboard = text.Substring(selectionStartPosition, selectionEndPosition - selectionStartPosition);
			}
			else
			{
				Clipboard = string.Empty;
			}
		}

		/// <summary>Pastes clipboard text</summary>
		internal virtual void Paste()
		{
			string input = Clipboard;
			string processedInput = string.Empty;

			int length = input.Length;
			for (int i = 0; i < length; i++)
			{
				char c = input[i];

				if (c >= ' ' || c == '\t' || c == '\r' || c == 10 || c == '\n')
				{
					processedInput += c;
				}
			}

			if (!string.IsNullOrEmpty(processedInput))
			{
				Insert(processedInput);
			}
		}

		/// <summary>Cuts current text selection</summary>
		internal virtual void Cut()
		{
			if (!secure)
			{
				Clipboard = text.Substring(selectionStartPosition, selectionEndPosition - selectionStartPosition);
			}
			else
			{
				Clipboard = string.Empty;
			}

			if (selectionEndPosition > selectionStartPosition)
			{
				DeleteSelection();
			}
		}

		internal void SelectAll()
		{
			selectionStartPosition = 0;
			selectionEndPosition = text.Length;
			OnTextEditUpdate(text, selectionStartPosition, selectionEndPosition);
		}

		/// <summary>Moves caret to start of the text</summary>
		internal void MoveToStart()
		{
			selectionStartPosition = 0;
			selectionEndPosition = selectionStartPosition;
			OnTextEditUpdate(text, selectionStartPosition, selectionEndPosition);
		}

		/// <summary>Moves caret to end of the text</summary>
		internal void MoveToEnd()
		{
			selectionStartPosition = text.Length;
			selectionEndPosition = selectionStartPosition;
			OnTextEditUpdate(text, selectionStartPosition, selectionEndPosition);
		}

		/// <summary>Tries to insert a character</summary>
		/// <param name="c">The character to insert</param>
		internal void TryInsertChar(char c)
		{
			if (!IsValidChar(c))
			{
				return;
			}

			Insert(c.ToString());
		}

		/// <summary>Checks if character is valid</summary>
		/// <param name="c">The character to check</param>
		internal bool IsValidChar(char c)
		{
			if ((int)c == 127 || (int)c == 0) //Delete key on mac and zero char
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// Insert a string at caret position and handle Korean Jamo combination.
		/// </summary>
		/// <param name="input">The string to insert</param>
		internal virtual void Insert(string input)
		{
			if (selectionEndPosition > selectionStartPosition)
			{
				text = text.Remove(selectionStartPosition, selectionEndPosition - selectionStartPosition);
				selectionEndPosition = selectionStartPosition;
			}

			string resultText;
			int caretPosition = selectionStartPosition;

			if (emojisAllowed) // Not validating individual characters when using emojis
			{
				resultText = text.Insert(caretPosition, input);
				caretPosition += input.Length;
			}
			else
			{
				if (characterLimit > 0 && text.Length + input.Length > characterLimit)
				{
					if (text.Length < characterLimit)
					{
						int amountAllowed = characterLimit - text.Length;
						input = input.Substring(0, amountAllowed);
					}
					else
					{
						return;
					}
				}

				// 한글 자모 조합 처리
				resultText = CombineKoreanJamo(text, input, ref caretPosition);
			}

			text = resultText;
			selectionStartPosition = caretPosition;
			selectionEndPosition = caretPosition;
			OnTextEditUpdate(text, selectionStartPosition, selectionEndPosition);
		}


		private string CombineKoreanJamo(string currentText, string input, ref int caretPosition)
		{
			var sb = new StringBuilder(currentText);

			// 1) 초·중·종성 테이블
			char[] L = { 'ㄱ', 'ㄲ', 'ㄴ', 'ㄷ', 'ㄸ', 'ㄹ', 'ㅁ', 'ㅂ', 'ㅃ', 'ㅅ', 'ㅆ', 'ㅇ', 'ㅈ', 'ㅉ', 'ㅊ', 'ㅋ', 'ㅌ', 'ㅍ', 'ㅎ' };
			char[] V = { 'ㅏ', 'ㅐ', 'ㅑ', 'ㅒ', 'ㅓ', 'ㅔ', 'ㅕ', 'ㅖ', 'ㅗ', 'ㅘ', 'ㅙ', 'ㅚ', 'ㅛ', 'ㅜ', 'ㅝ', 'ㅞ', 'ㅟ', 'ㅠ', 'ㅡ', 'ㅢ', 'ㅣ' };
			char[] T = {
							'\0','ㄱ','ㄲ','ㄳ','ㄴ','ㄵ','ㄶ','ㄷ','ㄹ','ㄺ','ㄻ','ㄼ','ㄽ','ㄾ','ㄿ','ㅀ',
							'ㅁ','ㅂ','ㅄ','ㅅ','ㅆ','ㅇ','ㅈ','ㅊ','ㅋ','ㅌ','ㅍ','ㅎ'
						};

			// 2) 복합 모음 매핑: (기존 중성 idx, 입력 중성 idx) → 새로운 중성 idx
			var compoundVowels = new Dictionary<(int, int), int>{
																	{(8,0),9},   {(8,1),10},  {(8,20),11},
																	{(13,4),14}, {(13,5),15}, {(13,20),16},
																	{(18,20),19}
																};

			// 3) composite-final 조합 매핑: (기존받침, 입력받침) → 새로운 composite 받침
			var combineDoubleFinals = new Dictionary<(char, char), char>
			{
				{('ㄱ','ㅅ'),'ㄳ'},
				{('ㄴ','ㅈ'),'ㄵ'},{('ㄴ','ㅎ'),'ㄶ'},
				{('ㄹ','ㄱ'),'ㄺ'},{('ㄹ','ㅁ'),'ㄻ'},{('ㄹ','ㅂ'),'ㄼ'},{('ㄹ','ㅅ'),'ㄽ'},{('ㄹ','ㅌ'),'ㄾ'},{('ㄹ','ㅍ'),'ㄿ'},{('ㄹ','ㅎ'),'ㅀ'},
				{('ㅂ','ㅅ'),'ㅄ'}
			};

			// 4) composite-final 분리 매핑: composite 받침 → (첫번째받침, 두번째초성)
			var splitDoubleFinals = new Dictionary<char, (char first, char second)>
			{
				{'ㄳ',('ㄱ','ㅅ')}, {'ㄵ',('ㄴ','ㅈ')}, {'ㄶ',('ㄴ','ㅎ')},
				{'ㄺ',('ㄹ','ㄱ')}, {'ㄻ',('ㄹ','ㅁ')}, {'ㄼ',('ㄹ','ㅂ')}, {'ㄽ',('ㄹ','ㅅ')}, {'ㄾ',('ㄹ','ㅌ')}, {'ㄿ',('ㄹ','ㅍ')}, {'ㅀ',('ㄹ','ㅎ')},
				{'ㅄ',('ㅂ','ㅅ')}
			};

			foreach (char ch in input)
			{
				// A) 이전 글자가 완성형 한글이고, 새 입력이 jamo(자음·모음)이면
				if (caretPosition > 0)
				{
					char prev = sb[caretPosition - 1];
					int sIndex = prev - 0xAC00;
					if (sIndex >= 0 && sIndex < 11172)
					{
						int lIdx = sIndex / (21 * 28);
						int vIdx = (sIndex % (21 * 28)) / 28;
						int tIdx = sIndex % 28;
						int v2 = Array.IndexOf(V, ch);  // 모음 인덱스
						int t2 = Array.IndexOf(T, ch);  // 자음(받침) 인덱스

						// A1) composite-final **조합** (길 + ㄱ → 긹)
						if (tIdx > 0 && t2 > 0 &&
							combineDoubleFinals.TryGetValue((T[tIdx], ch), out char newFinal))
						{
							int newTidx = Array.IndexOf(T, newFinal);
							sb[caretPosition - 1] = (char)(0xAC00 + (lIdx * 21 + vIdx) * 28 + newTidx);
							continue;
						}

						// A2) composite-final **분리** (긹 + ㅔ → 길 + 게)
						if (tIdx > 0 && v2 >= 0 &&
							splitDoubleFinals.TryGetValue(T[tIdx], out var pair))
						{
							// 앞글자: 기존 받침만 떼고
							int baseTidx = Array.IndexOf(T, pair.first);
							char baseSyl = (char)(0xAC00 + (lIdx * 21 + vIdx) * 28 + baseTidx);

							// 뒷글자: composite 둘째 자모를 초성으로 + 새 모음
							int nextLidx = Array.IndexOf(L, pair.second);
							char nextSyl = (char)(0xAC00 + (nextLidx * 21 + v2) * 28);

							sb[caretPosition - 1] = baseSyl;
							sb.Insert(caretPosition, nextSyl);
							caretPosition++;
							continue;
						}

						// A3) 단일 받침 분리 (안 + ㅏ → 아 + 나)
						if (tIdx > 0 && v2 >= 0)
						{
							char baseSyl = (char)(0xAC00 + (lIdx * 21 + vIdx) * 28);
							int nextL = Array.IndexOf(L, T[tIdx]);
							char nextSyl = (char)(0xAC00 + (nextL * 21 + v2) * 28);

							sb[caretPosition - 1] = baseSyl;
							sb.Insert(caretPosition, nextSyl);
							caretPosition++;
							continue;
						}

						// A4) 복합 모음 합성 (도 + ㅣ → 되)
						if (tIdx == 0 && v2 >= 0 &&
							compoundVowels.TryGetValue((vIdx, v2), out int newV))
						{
							sb[caretPosition - 1] = (char)(0xAC00 + (lIdx * 21 + newV) * 28);
							continue;
						}

						// A5) 단일 받침 추가 (가 + ㄱ → 각)
						if (tIdx == 0 && t2 > 0)
						{
							sb[caretPosition - 1] = (char)(0xAC00 + (lIdx * 21 + vIdx) * 28 + t2);
							continue;
						}
					}
				}

				// B) 호환 Jamo 초성+중성 합성 (ㅇ + ㅏ → 아, ㄷ + ㅣ → 디)
				if (caretPosition > 0 &&
					Array.IndexOf(L, sb[caretPosition - 1]) >= 0 &&
					Array.IndexOf(V, ch) >= 0)
				{
					int l = Array.IndexOf(L, sb[caretPosition - 1]);
					int v = Array.IndexOf(V, ch);
					sb[caretPosition - 1] = (char)(0xAC00 + (l * 21 + v) * 28);
					continue;
				}

				// C) 기타: 그냥 삽입
				sb.Insert(caretPosition, ch);
				caretPosition++;
			}

			return sb.ToString();
		}

		public void ApplyCharacterLimit(ref string text, ref int caretPosition)
		{
			if (characterLimit != 0 && text.Length > characterLimit)
			{
				text = text.Substring(0, characterLimit);
				caretPosition = Mathf.Clamp(caretPosition, 0, text.Length);
			}
		}

		/// <summary>Deletes previous character</summary>
		internal void DeletePreviousChar()
		{
			if (selectionEndPosition > selectionStartPosition)
			{
				DeleteSelection();
			}
			else if (selectionStartPosition > 0) //Backwards delete
			{
				selectionStartPosition--;

				EmojiData emojiData;
				if (emojisAllowed && NativeKeyboardManager.EmojiEngine.TryFindPreviousEmojiInText(text, selectionStartPosition, out emojiData))
				{
					int count = emojiData.text.Length;
					text = text.Remove(selectionStartPosition + 1 - count, count);
					selectionStartPosition -= (count - 1);
				}
				else
				{
					text = text.Remove(selectionStartPosition, 1);
				}
				selectionEndPosition = selectionStartPosition;

				OnTextEditUpdate(text, selectionStartPosition, selectionEndPosition);
			}
		}

		/// <summary>Deletes next character</summary>
		internal void DeleteNextChar()
		{
			if (selectionEndPosition > selectionStartPosition)
			{
				DeleteSelection();
			}
			else if (selectionStartPosition < text.Length) //Forward delete
			{
				EmojiData emojiData;
				if (emojisAllowed && NativeKeyboardManager.EmojiEngine.TryFindNextEmojiInText(text, selectionStartPosition, out emojiData))
				{
					int count = emojiData.text.Length;
					text = text.Remove(selectionStartPosition, count);
				}
				else
				{
					text = text.Remove(selectionStartPosition, 1);
				}
				selectionEndPosition = selectionStartPosition;

				OnTextEditUpdate(text, selectionStartPosition, selectionEndPosition);
			}
		}

		/// <summary>Deletes current text selection</summary>
		internal virtual void DeleteSelection()
		{
			text = text.Remove(selectionStartPosition, selectionEndPosition - selectionStartPosition);
			selectionEndPosition = selectionStartPosition;

			OnTextEditUpdate(text, selectionStartPosition, selectionEndPosition);
		}
	}
}
