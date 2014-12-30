package com.my.home.Services;

/**
 * Created by Hristo on 13-12-28.
 */
public enum EServiceType {
    Invalid(0),
    PCControl(1),
    HomeControl(2),
	TVControl(3);

    private final int value;

    private EServiceType(int value) {
        this.value = value;
    }

    public int getValue() {
        return this.value;
    }
}
