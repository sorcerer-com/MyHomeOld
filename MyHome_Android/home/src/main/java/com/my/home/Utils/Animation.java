package com.my.home.Utils;

import android.graphics.Bitmap;
import android.graphics.Rect;
import android.os.Handler;
import android.os.Looper;
import android.view.View;
import android.widget.ImageView;
import android.widget.LinearLayout;
import android.widget.TextView;

import com.my.home.MainActivity;

/**
 * Created by Hristo on 14-04-06.
 */
public class Animation {

	public static void openMenu(final LinearLayout linearLayout, final boolean open, final int delay) {
		WorkerThread.execute( new Runnable() {
            @Override
            public void run() {
                Handler handler = new Handler(Looper.getMainLooper());
                if (open) {
					handler.post( new Runnable() {
                        @Override
                        public void run() {
                            for (int i = 0; i < linearLayout.getChildCount(); i++)
                                linearLayout.getChildAt(i).setVisibility(View.GONE);
                            linearLayout.setVisibility(View.VISIBLE);
                        }
                    });
                }

                for (int i = 0; i < linearLayout.getChildCount(); i++) {
                    final int idx = open ? i : linearLayout.getChildCount() - i - 1;

                    Object tag = linearLayout.getChildAt(idx).getTag();
                    if (!MainActivity.isServiceAvailable(tag))
                        continue;
					handler.post( new Runnable() {
                        @Override
                        public void run() {
                            if (open)
                                linearLayout.getChildAt(idx).setVisibility(View.VISIBLE);
                            else
                                linearLayout.getChildAt(idx).setVisibility(View.GONE);
                        }
                    });
                    Utils.Thread.sleep(delay);
                }

                if (!open) {
					handler.post( new Runnable() {
                        @Override
                        public void run() {
                            linearLayout.setVisibility(View.GONE);
                            for (int i = 0; i < linearLayout.getChildCount(); i++)
                                linearLayout.getChildAt(i).setVisibility(View.VISIBLE);
                        }
                    });
                }
            }
        });
	}

    public static void switchText(final TextView textView, final int delay, final String... texts) {
        WorkerThread.execute( new Runnable() {
			private int idx = 0;
			
            @Override
            public void run() {
				if (!textView.getText().toString().equals(texts[idx]))
					return;
				idx = (idx + 1) % texts.length;

                Handler handler = new Handler(Looper.getMainLooper());
                handler.post( new Runnable() {
                    @Override
                    public void run() {
						textView.setText(texts[idx]);
                    }
                });
				
                Utils.Thread.sleep(delay);
                WorkerThread.execute(this);
            }
        });
    }
	
	public static void fade(final ImageView imageView, final int time, final boolean out) {
		final int numCycles = time / 50;

        WorkerThread.execute( new Runnable() {
			private int i = 0;
			
            @Override
            public void run() {
				i++;
				if (i > numCycles + 1)
					return;

				final int alpha = Math.min(i * (255 / numCycles), 255);
                Handler handler = new Handler(Looper.getMainLooper());
                handler.post( new Runnable() {
                    @Override
                    public void run() {
                        if (out)
                            imageView.setAlpha(255 - alpha);
                        else
                            imageView.setAlpha(alpha);
                    }
                });
				
                Utils.Thread.sleep(time / numCycles);
                WorkerThread.execute(this);
            }
        });
	}
	
	public static void zoom(final ImageView imageView, final Bitmap originalBitmap, final int time, final Rect start, final Rect end) {
		final int numCycles = time / 50;
		final int dx = (end.left - start.left) / numCycles;
		final int dy = (end.top - start.top) / numCycles;
		final int dw = (end.width() - start.width()) / numCycles;
		final int dh = (end.height() - start.height()) / numCycles;
		
        WorkerThread.execute( new Runnable() {
			private int i = 0;
			
            @Override
            public void run() {
				i++;
				if (i > numCycles + 1)
					return;

                int x = Math.max(start.left + dx * i, 0);
                int y = Math.max(start.top + dy * i, 0);
                int width = Math.min(start.width() + dw * i, originalBitmap.getWidth() - x);
                int height = Math.min(start.height() + dh * i, originalBitmap.getHeight() - y);
				final Bitmap bitmap = Bitmap.createBitmap(originalBitmap, x, y, width, height);

                Handler handler = new Handler(Looper.getMainLooper());
                handler.post( new Runnable() {
                    @Override
                    public void run() {
						imageView.setImageBitmap(bitmap);
                    }
                });
				
                Utils.Thread.sleep(time / numCycles);
                WorkerThread.execute(this);
            }
        });
		
	}

}
