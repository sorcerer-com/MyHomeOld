package com.my.home.Activities;

import android.app.Activity;
import android.content.Context;
import android.graphics.Point;
import android.os.Bundle;
import android.view.KeyEvent;
import android.view.MenuItem;
import android.view.MotionEvent;
import android.view.View;
import android.view.inputmethod.InputMethodManager;
import android.widget.CheckBox;
import android.widget.ImageView;

import com.my.home.R;
import com.my.home.Services.EServiceType;
import com.my.home.Services.PCControl;
import com.my.home.Services.Service;
import com.my.home.Services.ServiceManager;
import com.my.home.Utils.Enums;
import com.my.home.Utils.Utils;

public class PCControlActivity extends Activity implements View.OnTouchListener, Service.PropertyChangedListener {

	private int downPointersCount;
	private Point lastTouchPos;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_pccontrol);

        View imageView = findViewById(R.id.pccontrol_screen_imageview);
        imageView.setOnTouchListener(this);

		Service.registerPropertyChangedListener(this);

        PCControl.Send.getMousePosition();
    }
	
    @Override
    protected void onDestroy() {
		Service.unregisterPropertyChangedListener(this);
        super.onDestroy();
    }

    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        switch (item.getItemId()) {
            case android.R.id.home:
                onBackPressed();
                return true;
        }

        return super.onOptionsItemSelected(item);
    }

    // key press handler
    @Override
    public boolean dispatchKeyEvent(KeyEvent e) {
        if (e.getAction() == KeyEvent.ACTION_DOWN && e.getKeyCode() != KeyEvent.KEYCODE_BACK) {
            PCControl.Send.setKey(e.getUnicodeChar());

            return true;
        }
        return super.dispatchKeyEvent(e);
    }

    @Override
    public boolean onTouch(View view, MotionEvent motionEvent) {
        PCControl pcControl = Utils.as(ServiceManager.getService(EServiceType.PCControl), PCControl.class);
		if (pcControl == null)
			return false;

		Point touchPos = new Point((int)motionEvent.getX(), (int)motionEvent.getY());
			
        if (motionEvent.getAction() == MotionEvent.ACTION_DOWN) {
            this.downPointersCount = motionEvent.getPointerCount();
			PCControl.Send.getMousePosition();
            this.lastTouchPos = touchPos;
            return true;
        }

		// move mouse
		if (motionEvent.getAction() == MotionEvent.ACTION_MOVE && motionEvent.getPointerCount() == 1) {
			if (!pcControl.getMousePos().equals(0, 0)) {
                // TODO: some kind of repeat bug
				Point mousePos = pcControl.getMousePos();
				mousePos.x += (touchPos.x - this.lastTouchPos.x);
				mousePos.y += (touchPos.y - this.lastTouchPos.y);

				pcControl.setMousePos(mousePos);
			}
			else 
				PCControl.Send.getMousePosition();
		}
		
		// scroll
		if (motionEvent.getAction() == MotionEvent.ACTION_MOVE && motionEvent.getPointerCount() == 2) {
			int delta = (int)(motionEvent.getY() - this.lastTouchPos.y) * 10;
            PCControl.Send.setMouseWheel(-delta);
		}

		// click Left / Right button
		if (motionEvent.getActionMasked() == MotionEvent.ACTION_POINTER_DOWN)
			this.downPointersCount = motionEvent.getPointerCount();
		if (motionEvent.getAction() == MotionEvent.ACTION_UP) {
            long duration = motionEvent.getEventTime() - motionEvent.getDownTime();
			if (duration < 500) {
				Enums.EMouseButton mouseButton = Enums.EMouseButton.Left;
				if (this.downPointersCount == 1)
					mouseButton = Enums.EMouseButton.Left;
				else if (this.downPointersCount == 2)
					mouseButton = Enums.EMouseButton.Right;
				else if (this.downPointersCount == 3)
					mouseButton = Enums.EMouseButton.Middle;
					
				if (motionEvent.getPointerCount() <= 3) {
					PCControl.Send.setMouseButton(mouseButton, Enums.EMouseButtonState.Pressed);
					Utils.Thread.sleep(100);
					PCControl.Send.setMouseButton(mouseButton, Enums.EMouseButtonState.Released);
				}
			}
		}

        this.lastTouchPos = touchPos;

		Utils.Thread.sleep(10);
        return true;
    }
	
	
    @Override
    public void onPropertyChanged(Service service, String property) {
        final PCControl pcControl = Utils.as(service, PCControl.class);
        if (pcControl == null)
            return;

        if (property.equals("PCControl.MousePos")) {
			Point mousePos = pcControl.getMousePos();
		
			CheckBox getScreenCheckBox = (CheckBox)findViewById(R.id.pccontrol_getscreen_checkbox);
			if (getScreenCheckBox.isChecked()) {
				ImageView imageView = (ImageView)findViewById(R.id.pccontrol_screen_imageview);
				int width = imageView.getWidth();
				int height = imageView.getHeight();
				if (width == 0) width = 4;
				if (height == 0) height = 4;
				
				PCControl.Send.getScreenImage(mousePos.x - width / 4, mousePos.y - height / 4, width / 2, height / 2);
			}
		}
		else if (property.equals("PCControl.ScreenImage")) {
            ImageView imageView = (ImageView)findViewById(R.id.pccontrol_screen_imageview);
            imageView.setImageBitmap(pcControl.getScreenImage());

			//Utils.Thread.sleep(1000);
            PCControl.Send.getMousePosition();
		}
		else if (property.equals("PCControl.CameraImage")) {
			ImageView imageView = (ImageView)findViewById(R.id.pccontrol_screen_imageview);
			imageView.setImageBitmap(pcControl.getCameraImage());

			CheckBox getCameraCheckBox = (CheckBox)findViewById(R.id.pccontrol_getcamera_checkbox);
			if (getCameraCheckBox.isChecked()) {
                //Utils.Thread.sleep(1000);
                PCControl.Send.getCameraImage();
			}
		}
	}


    public void keyboardImageView_Click(View view) {
		// show keyboard even if there is hardware one
        //Configuration config = this.getResources().getConfiguration();
        //if (config.hardKeyboardHidden == Configuration.HARDKEYBOARDHIDDEN_YES) {
            InputMethodManager imm = (InputMethodManager)this.getSystemService(Context.INPUT_METHOD_SERVICE);
            imm.toggleSoftInputFromWindow(view.getApplicationWindowToken(), InputMethodManager.SHOW_IMPLICIT, 0);
        //}
    }

    public void getImageCheckBox_Click(View view) {
        CheckBox checkBox = (CheckBox)view;
        if (checkBox.getId() == R.id.pccontrol_getscreen_checkbox && checkBox.isChecked()) {
			CheckBox getCameraCheckBox = (CheckBox)findViewById(R.id.pccontrol_getcamera_checkbox);
			getCameraCheckBox.setChecked(false);
		
            PCControl.Send.getMousePosition();
		}
        else if (checkBox.getId() == R.id.pccontrol_getcamera_checkbox && checkBox.isChecked())	{
			CheckBox getCameraCheckBox = (CheckBox)findViewById(R.id.pccontrol_getscreen_checkbox);
			getCameraCheckBox.setChecked(false);
		
			PCControl.Send.getCameraImage();
		}
    }

}
