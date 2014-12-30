package com.my.home.Activities;

import android.app.Activity;
import android.content.Context;
import android.os.Bundle;
import android.view.Gravity;
import android.view.MenuItem;
import android.view.View;
import android.view.ViewGroup;
import android.widget.AdapterView;
import android.widget.BaseAdapter;
import android.widget.GridView;
import android.widget.ImageView;
import android.widget.LinearLayout;
import android.widget.ListView;
import android.widget.TextView;

import com.my.home.MainActivity;
import com.my.home.R;
import com.my.home.Services.EServiceType;
import com.my.home.Services.Service;
import com.my.home.Services.ServiceManager;
import com.my.home.Services.TVControl;
import com.my.home.Utils.Utils;

import java.util.ArrayList;

public class TVControlActivity extends Activity implements Service.PropertyChangedListener {

	private String mode;
    private String currDir;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_tvcontrol);
		
        Service.registerPropertyChangedListener(this);
		TVControl.Send.getTelevisions();
		this.mode = "television";
		this.currDir = " ";

        Service.onPropertyChanged(EServiceType.TVControl, "TVControl.Televisions");
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
	

    public void menuItemImageView_Click(View view) {
        if (view == null || view.getContentDescription() == null)
            return;

		String desc = view.getContentDescription().toString();
		if (desc.equals(this.getResources().getString(R.string.tvcontrol_remotecontrol))) {
            Service.onPropertyChanged(EServiceType.TVControl, "TVControl.Televisions");
		}
		else if (desc.equals(this.getResources().getString(R.string.tvcontrol_movies))) {
            Service.onPropertyChanged(EServiceType.TVControl, "TVControl.Movies");
		}
		else if (desc.equals(this.getResources().getString(R.string.tvcontrol_images))) {
            Service.onPropertyChanged(EServiceType.TVControl, "TVControl.Images");
		}
		// Control Menu
		else if (!this.mode.equals("television")) {
			String command = null;
			if (desc.equals(this.getResources().getString(R.string.tvcontrol_prev))) {
				command = "prev";
			}
			else if (desc.equals(this.getResources().getString(R.string.tvcontrol_play))) {
				command = "play";
				ImageView menuItemImageView = (ImageView)view;
				menuItemImageView.setImageResource(R.drawable.ic_tvcontrol_pause);
				menuItemImageView.setContentDescription(this.getResources().getString(R.string.tvcontrol_pause));
			}
			else if (desc.equals(this.getResources().getString(R.string.tvcontrol_pause))) {
				command = "pause";
				ImageView menuItemImageView = (ImageView)view;
				menuItemImageView.setImageResource(R.drawable.ic_tvcontrol_play);
				menuItemImageView.setContentDescription(this.getResources().getString(R.string.tvcontrol_play));
			}
			else if (desc.equals(this.getResources().getString(R.string.tvcontrol_stop))) {
				command = "stop";
			}
			else if (desc.equals(this.getResources().getString(R.string.tvcontrol_next))) {
				command = "next";
			}
			
			if (this.mode.equals("movie"))
				TVControl.Send.setMovie(command, "");
			else
				TVControl.Send.setImage(command, "");
		}
    }
	
	
    @Override
    public void onPropertyChanged(Service service, String property) {
        final TVControl tvControl = Utils.as(service, TVControl.class);
        if (tvControl == null)
            return;
			
        if (property.equals("TVControl.Televisions")) {
			TVControl.Television television = null;
			ArrayList<TVControl.Television> televisions = tvControl.getTelevisions();
			for (TVControl.Television tv : televisions)
				if (tv.roomId == MainActivity.SelectedRoom.id) {
					television = tv;
					break;
				}
			
			if (television != null) {
				int size = television.remoteControl.size();
                ArrayList<String> buttons = new ArrayList<String>();
				for (int i = 0; i < size; i++)
					buttons.add(television.remoteControl.get(i).name);
				
				ListView listView = (ListView)findViewById(R.id.tvcontrol_listview);
				listView.setVisibility(View.GONE);
				
				GridView gridView = (GridView)findViewById(R.id.tvcontrol_gridview);
				gridView.setVisibility(View.VISIBLE);
				gridView.setAdapter(new ContentAdapter(this, buttons));
				gridView.setOnItemClickListener(new AdapterView.OnItemClickListener() {
					public void onItemClick(AdapterView<?> parent, View v, int position, long id) {
						TVControl.Send.setRemoteControlButton(MainActivity.SelectedRoom.id, position);
					}
				});
				this.mode = "television";
				LinearLayout controlMenuLinearLayout = (LinearLayout)findViewById(R.id.tvcontrol_control_menu);
				controlMenuLinearLayout.setVisibility(View.GONE);
			}
		}
		else if (property.equals("TVControl.Movies")) {
			GridView gridView = (GridView)findViewById(R.id.tvcontrol_gridview);
			gridView.setVisibility(View.GONE);
			
			ListView listView = (ListView)findViewById(R.id.tvcontrol_listview);
			listView.setVisibility(View.VISIBLE);
			listView.setAdapter(new ContentAdapter(this, tvControl.getMovies()));
			listView.setOnItemClickListener(new AdapterView.OnItemClickListener() {
				public void onItemClick(AdapterView<?> parent, View v, int position, long id) {
					String movie = (String)parent.getAdapter().getItem(position);
					if (movie.endsWith(" (sub)"))
						movie = movie.substring(0, movie.length() - 6);
					TVControl.Send.setMovie("open", movie);
				}
			});
			this.mode = "movie";
			LinearLayout controlMenuLinearLayout = (LinearLayout)findViewById(R.id.tvcontrol_control_menu);
			controlMenuLinearLayout.setVisibility(View.VISIBLE);
		}
		else if (property.equals("TVControl.Images")) {

			GridView gridView = (GridView)findViewById(R.id.tvcontrol_gridview);
			gridView.setVisibility(View.GONE);

			ArrayList<String> items = new ArrayList<String>(tvControl.getImages());
			final char separator = items.size () > 0 ? items.get(0).charAt(0) : ' ';
			if (this.currDir.equals(" ") && separator != ' ')
				this.currDir = "" + separator;
			if (this.currDir.lastIndexOf(separator) > 0) // if currDir isn't the root
				items.add(0, this.currDir + "..." + separator);
			final ListView listView = (ListView)findViewById(R.id.tvcontrol_listview);
			listView.setVisibility(View.VISIBLE);
			listView.setAdapter(new ContentAdapter(this, items));
			listView.setOnItemClickListener(new AdapterView.OnItemClickListener() {
				public void onItemClick(AdapterView<?> parent, View v, int position, long id) {
					String item = (String)parent.getAdapter().getItem(position);
					if (item.endsWith("" + separator)) { // check for the folder
						if (item.endsWith("..." + separator)) { // go to parent directory
							TVControlActivity.this.currDir = TVControlActivity.this.currDir.substring(0, TVControlActivity.this.currDir.length() - 1);
							TVControlActivity.this.currDir = TVControlActivity.this.currDir.substring(0, TVControlActivity.this.currDir.lastIndexOf(separator) + 1);
						}
						else
                            TVControlActivity.this.currDir += item.substring(1);
						
						TVControl.Send.getImages(TVControlActivity.this.currDir);
					}
					else {
						String image = TVControlActivity.this.currDir + item.substring(1);
						TVControl.Send.setImage("open", image);
					}
				}
			});
			this.mode = "image";
			LinearLayout controlMenuLinearLayout = (LinearLayout)findViewById(R.id.tvcontrol_control_menu);
			controlMenuLinearLayout.setVisibility(View.VISIBLE);
		}
	}
	
	
	private class ContentAdapter extends BaseAdapter {
		private final Context context;
		private final ArrayList<String> values;
		
		public ContentAdapter(Context context, ArrayList<String> values) {
			this.context = context;
			this.values = values;
		}

		@Override
		public View getView(int position, View convertView, ViewGroup parent) {
			TextView textView;
			if (convertView == null) { // if it's not recycled, initialize some attributes
				textView = new TextView(this.context);
                textView.setGravity(Gravity.CENTER);
			}
			else {
				textView = (TextView)convertView;
			}
			
			textView.setText(this.values.get(position));
			return textView;
		}

		@Override
		public int getCount() {
			return this.values.size();
		}
	 
		@Override
		public Object getItem(int position) {
			return this.values.get(position);
		}

        @Override
        public long getItemId(int i) {
            return 0;
        }

    }

}
