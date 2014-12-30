package com.my.home.Utils;

/**
 * Created by Hristo on 14-02-05.
 */
public class Enums {

	public static enum EMouseButton {
		Left(0),
		Middle(1),
		Right(2);

		private final int value;

		private EMouseButton(int value) {
			this.value = value;
		}

		public int getValue() {
			return this.value;
		}
	}
	
	public static enum EMouseButtonState {
		Released(0),
		Pressed(1);

		private final int value;

		private EMouseButtonState(int value) {
			this.value = value;
		}

		public int getValue() {
			return this.value;
		}
	}
}