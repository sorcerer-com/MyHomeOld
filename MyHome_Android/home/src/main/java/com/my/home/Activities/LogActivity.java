package com.my.home.Activities;

import android.app.Activity;
import android.os.Bundle;
import android.view.MenuItem;
import android.widget.TextView;

import com.my.home.R;
import com.my.home.TcpConnection.Client;

public class LogActivity extends Activity {

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_log);

		String s = "";
		int size = Client.Log.size();
		for (int i = 0; i < size; i++)
			s += Client.Log.get(i) + "\n";
		TextView textView = (TextView) findViewById(R.id.log_textview);
		textView.setText(s);
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

}
