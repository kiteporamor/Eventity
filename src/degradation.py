#-----------------------------------------HTML------------------------------------------------------
import pandas as pd
import numpy as np
import plotly.graph_objects as go
from plotly.subplots import make_subplots
import plotly.express as px
from datetime import datetime, timedelta
import os
from IPython.display import display, HTML
import warnings
warnings.filterwarnings('ignore')

def save_plotly_fig(fig, filename, path='.'):
    """Save Plotly figure as HTML file"""
    output_path = os.path.join(path, filename)
    fig.write_html(output_path)
    print(f"Graph saved as: {output_path}")
    return output_path

def load_large_csv_in_chunks(file_path, chunk_size=100000):
    """Load large CSV file in chunks to avoid memory issues"""
    print(f"Loading large CSV file in chunks: {file_path}")
    chunks = []
    total_rows = 0
    
    try:
        for chunk in pd.read_csv(file_path, chunksize=chunk_size):
            chunks.append(chunk)
            total_rows += len(chunk)
            print(f"Loaded chunk {len(chunks)}: {len(chunk)} rows (total: {total_rows})")
        
        if chunks:
            return pd.concat(chunks, ignore_index=True)
        else:
            return pd.DataFrame()
    except Exception as e:
        print(f"Error loading CSV: {e}")
        return pd.DataFrame()

def optimize_dataframe_memory(df):
    """Optimize dataframe memory usage"""
    if df.empty:
        return df
        
    print("Optimizing dataframe memory usage...")
    initial_memory = df.memory_usage(deep=True).sum() / 1024**2
    
    # Convert numeric columns to appropriate types
    numeric_columns = ['metric_value', 'timestamp']
    for col in numeric_columns:
        if col in df.columns:
            df[col] = pd.to_numeric(df[col], errors='coerce', downcast='float')
    
    # Convert categorical columns
    categorical_columns = ['metric_name', 'status', 'error', 'container', 'cpu_percent', 'mem_usage']
    for col in categorical_columns:
        if col in df.columns and df[col].dtype == 'object':
            df[col] = df[col].astype('category')
    
    final_memory = df.memory_usage(deep=True).sum() / 1024**2
    print(f"Memory optimization: {initial_memory:.2f} MB -> {final_memory:.2f} MB")
    
    return df

def debug_data_structure(csv_data):
    """Function for debugging data structure"""
    print("\nDEBUG DATA STRUCTURE:")
    print(f"Total rows: {len(csv_data)}")
    print(f"Columns: {list(csv_data.columns)}")
    
    if not csv_data.empty:
        print(f"Metric types: {csv_data['metric_name'].unique()}")
        
        # Look at error data
        error_data = csv_data[csv_data['metric_name'] == 'http_req_failed']
        if not error_data.empty:
            print(f"\nHTTP_REQ_FAILED data:")
            print(f"   - Number of records: {len(error_data)}")
            print(f"   - Values: {error_data['metric_value'].unique()[:10]}")
        
        # Look at HTTP request statuses
        http_requests = csv_data[csv_data['metric_name'] == 'http_req_duration']
        if not http_requests.empty and 'status' in http_requests.columns:
            print(f"\nHTTP statuses:")
            print(f"   - Unique statuses: {http_requests['status'].unique()}")
            print(f"   - Example statuses: {http_requests['status'].value_counts().head()}")
        
        # Look at http_reqs data
        http_reqs = csv_data[csv_data['metric_name'] == 'http_reqs']
        if not http_reqs.empty:
            print(f"\nHTTP_REQS data:")
            print(f"   - Number of records: {len(http_reqs)}")
            print(f"   - Values: {http_reqs['metric_value'].unique()[:10]}")
            if 'timestamp' in http_reqs.columns:
                print(f"   - Timestamps: {http_reqs['timestamp'].head(3)}")
        
        # Sample data for debugging
        print(f"\nFirst 3 rows:")
        print(csv_data.head(3))
        
        # Memory usage info
        print(f"\nMemory usage: {csv_data.memory_usage(deep=True).sum() / 1024**2:.2f} MB")

def calculate_error_rate_chunked(csv_data, time_window, window_start, window_end):
    """Calculate error rate using chunked processing for large datasets"""
    try:
        # Filter HTTP requests in this interval
        http_requests = csv_data[
            (csv_data['metric_name'] == 'http_req_duration')
        ].copy()
        
        if http_requests.empty:
            return 0, 0
        
        # Convert timestamp for filtering
        http_requests.loc[:, 'timestamp_dt'] = pd.to_datetime(
            http_requests['timestamp'], unit='s', errors='coerce'
        )
        http_requests = http_requests.dropna(subset=['timestamp_dt'])
        
        # Filter by time window
        window_requests = http_requests[
            (http_requests['timestamp_dt'] >= window_start) & 
            (http_requests['timestamp_dt'] < window_end)
        ]
        
        total_requests = len(window_requests)
        
        if total_requests == 0:
            return 0, 0
        
        # Count errors by status (4xx, 5xx) or presence of error
        error_count = 0
        
        if 'status' in window_requests.columns:
            # Process in chunks if too large
            if len(window_requests) > 100000:
                chunk_size = 50000
                for i in range(0, len(window_requests), chunk_size):
                    chunk = window_requests.iloc[i:i+chunk_size]
                    status_series = pd.to_numeric(chunk['status'], errors='coerce')
                    error_chunk = chunk[
                        (status_series >= 400) |  # HTTP errors
                        (chunk['error'].notna())  # Has error text
                    ]
                    error_count += len(error_chunk)
            else:
                status_series = pd.to_numeric(window_requests['status'], errors='coerce')
                error_requests = window_requests[
                    (status_series >= 400) |  # HTTP errors
                    (window_requests['error'].notna())  # Has error text
                ]
                error_count = len(error_requests)
        
        error_rate = (error_count / total_requests * 100) if total_requests > 0 else 0
        
        return error_count, error_rate
        
    except Exception as e:
        print(f"Error calculating errors: {e}")
        return 0, 0

def process_duration_data_chunked(csv_data, sample_fraction=0.1):
    """Process duration data with sampling for large datasets"""
    print("Processing duration data with sampling...")
    
    duration_data = csv_data[csv_data['metric_name'] == 'http_req_duration']
    
    if duration_data.empty:
        print("No response time data")
        return pd.DataFrame()
    
    print(f"Original duration data: {len(duration_data)} rows")
    
    # Sample data if too large
    if len(duration_data) > 100000:  # If more than 100K rows, sample
        print(f"Large dataset detected. Sampling {sample_fraction*100}%...")
        duration_data = duration_data.sample(frac=sample_fraction, random_state=42)
        print(f"After sampling: {len(duration_data)} rows")
    
    # Prepare data
    duration_data = duration_data.copy()
    duration_data['metric_value'] = pd.to_numeric(duration_data['metric_value'], errors='coerce')
    duration_data = duration_data.dropna(subset=['metric_value'])
    
    if 'timestamp' not in duration_data.columns:
        print("No timestamps for analysis")
        return pd.DataFrame()
    
    # Convert timestamp
    duration_data.loc[:, 'timestamp_dt'] = pd.to_datetime(
        duration_data['timestamp'], unit='s', errors='coerce'
    )
    duration_data = duration_data.dropna(subset=['timestamp_dt'])
    duration_data = duration_data.sort_values('timestamp_dt')
    
    print(f"Final processed duration data: {len(duration_data)} rows")
    return duration_data

def create_detailed_percentiles_chart(csv_data, output_path):
    """Creates detailed chart with 5 percentile functions over test time"""
    
    print("Creating detailed percentiles chart...")
    
    # Use sampling for large datasets
    duration_data = process_duration_data_chunked(csv_data, sample_fraction=0.05)
    
    if duration_data.empty:
        return
    
    # Prepare data
    duration_data = duration_data.copy()
    duration_data['timestamp'] = pd.to_numeric(duration_data['timestamp'], errors='coerce')
    duration_data = duration_data.dropna(subset=['timestamp'])
    
    if duration_data.empty:
        return
    
    # Sort by time and create time intervals
    duration_data = duration_data.sort_values('timestamp')
    duration_data['time_interval'] = (duration_data['timestamp'] - duration_data['timestamp'].min())
    
    # Use all available time
    max_time = duration_data['time_interval'].max()
    
    # Break into time intervals
    time_intervals = np.linspace(0, max_time, min(100, int(max_time) + 1))  # adaptive number of points
    percentiles = [50, 75, 90, 95, 99]
    colors = ['green', 'blue', 'orange', 'red', 'purple']
    labels = ['P50', 'P75', 'P90', 'P95', 'P99']
    
    # Calculate percentiles for each time interval
    percentile_over_time = {p: [] for p in percentiles}
    time_points = []
    
    for i in range(1, len(time_intervals)):
        time_start = time_intervals[i-1]
        time_end = time_intervals[i]
        
        # Data for current time interval
        interval_data = duration_data[
            (duration_data['time_interval'] >= time_start) & 
            (duration_data['time_interval'] < time_end)
        ]['metric_value']
        
        if len(interval_data) > 0:
            time_points.append(time_end)  # Use the end time point
            for p in percentiles:
                p_value = np.percentile(interval_data, p)
                percentile_over_time[p].append(p_value)
        else:
            # Add NaN if no data in interval
            time_points.append(time_end)
            for p in percentiles:
                percentile_over_time[p].append(np.nan)
    
    # Create interactive plot using Plotly
    fig = go.Figure()
    
    # Add each percentile as a line with markers
    for p, color, label in zip(percentiles, colors, labels):
        if len(percentile_over_time[p]) > 0:
            fig.add_trace(go.Scatter(
                x=time_points,
                y=percentile_over_time[p],
                mode='lines+markers',
                name=label,
                line=dict(width=3),
                marker=dict(size=5),
                hovertemplate='Time: %{x}s<br>'+label+': %{y}ms<extra></extra>'
            ))
    
    # Add degradation threshold
    fig.add_hline(y=500, line_dash="dash", line_color="black", line_width=2,
                  annotation_text="Degradation Threshold (500 ms)", 
                  annotation_font_size=12)
    
    fig.update_layout(
        title='Dynamics of Response Time Percentiles During Test',
        xaxis_title='Test Time (seconds)',
        yaxis_title='Response Time (milliseconds)',
        height=700,
        hovermode='x unified',
        font=dict(size=12)
    )
    
    # Save the figure
    save_plotly_fig(fig, "detailed_percentiles_chart.html", output_path)
    
    print(f"Detailed percentiles chart saved")
    print(f"Time range: 0-{max_time} seconds")

def create_cumulative_percentiles_chart(csv_data, output_path):
    """Creates interactive chart with cumulative percentiles (all data up to current moment)"""
    
    print("Creating cumulative percentiles chart...")
    
    # Use sampling for large datasets
    duration_data = process_duration_data_chunked(csv_data, sample_fraction=0.05)
    
    if duration_data.empty:
        return
    
    # Prepare data
    duration_data = duration_data.copy()
    duration_data['timestamp'] = pd.to_numeric(duration_data['timestamp'], errors='coerce')
    duration_data = duration_data.dropna(subset=['timestamp'])
    
    if duration_data.empty:
        return
    
    # Sort by time and create time intervals
    duration_data = duration_data.sort_values('timestamp')
    duration_data['time_interval'] = (duration_data['timestamp'] - duration_data['timestamp'].min())
    
    # Use all available time
    max_time = duration_data['time_interval'].max()
    
    # Break into time intervals
    time_intervals = np.linspace(0, max_time, min(100, int(max_time) + 1))
    percentiles = [50, 75, 90, 95, 99]
    colors = ['green', 'blue', 'orange', 'red', 'purple']
    labels = ['P50', 'P75', 'P90', 'P95', 'P99']
    
    # Calculate CUMULATIVE percentiles for each time interval
    percentile_over_time = {p: [] for p in percentiles}
    time_points = []
    
    for i in range(1, len(time_intervals)):
        time_end = time_intervals[i]
        
        # Take ALL data up to this point in time
        data_so_far = duration_data[duration_data['time_interval'] <= time_end]['metric_value']
        
        if len(data_so_far) > 0:
            time_points.append(time_end)
            for p in percentiles:
                p_value = np.percentile(data_so_far, p)
                percentile_over_time[p].append(p_value)
        else:
            time_points.append(time_end)
            for p in percentiles:
                percentile_over_time[p].append(np.nan)
    
    # Create interactive plot
    fig = go.Figure()
    
    # Add each cumulative percentile as a line with markers
    for p, color, label in zip(percentiles, colors, labels):
        if len(percentile_over_time[p]) > 0:
            fig.add_trace(go.Scatter(
                x=time_points,
                y=percentile_over_time[p],
                mode='lines+markers',
                name=label,
                line=dict(width=3),
                marker=dict(size=5),
                hovertemplate='Time: %{x}s<br>Cumulative '+label+': %{y}ms<extra></extra>'
            ))
    
    # Add degradation threshold
    fig.add_hline(y=500, line_dash="dash", line_color="black", line_width=2,
                  annotation_text="Degradation Threshold (500 ms)", 
                  annotation_font_size=12)
    
    fig.update_layout(
        title='Cumulative Response Time Percentiles (All Data Up to Current Moment)',
        xaxis_title='Test Time (seconds)',
        yaxis_title='Response Time (milliseconds)',
        height=700,
        hovermode='x unified',
        font=dict(size=12)
    )
    
    # Save the figure
    save_plotly_fig(fig, "cumulative_percentiles_chart.html", output_path)
    
    print(f"Cumulative percentiles chart saved")
    print(f"Time range: 0-{max_time} seconds")

def create_comprehensive_response_plots(csv_data, output_path):
    """Creates comprehensive response time plots with all percentiles"""
    
    print("Creating comprehensive response time plots...")
    
    # Sample data for large files
    duration_data = process_duration_data_chunked(csv_data, sample_fraction=0.05)
    
    if duration_data.empty:
        return
    
    # Create subplot with 2 charts
    fig = make_subplots(
        rows=2, cols=1,
        subplot_titles=['Response Time by Percentiles (Completed Requests)', 'Response Time Distribution (Completed Requests)'],
        vertical_spacing=0.12
    )
    
    # Plot 1: Response time by percentiles
    try:
        completed_requests = csv_data[csv_data['metric_name'] == 'http_req_duration']
        
        if not completed_requests.empty:
            percentiles = [50, 75, 90, 95, 99]
            percentile_values = [np.percentile(completed_requests['metric_value'], p) for p in percentiles]
            
            fig.add_trace(
                go.Bar(
                    x=[f'P{p}' for p in percentiles],
                    y=percentile_values,
                    name='Response Time by Percentiles',
                    marker_color=['lightgreen', 'lightblue', 'orange', 'red', 'darkred'],
                    hovertemplate='Percentile: %{x}<br>Response Time: %{y}ms<extra></extra>'
                ),
                row=1, col=1
            )
        else:
            fig.add_annotation(
                text="No completed request data",
                xref="x", yref="y",
                x=0.5, y=0.5,
                showarrow=False,
                row=1, col=1
            )
        
    except Exception as e:
        fig.add_annotation(
            text=f'Percentile error: {e}',
            xref="x", yref="y",
            x=0.5, y=0.5,
            showarrow=False,
            row=1, col=1
        )
    
    # Plot 2: Response time histogram
    try:
        completed_requests = csv_data[csv_data['metric_name'] == 'http_req_duration']
        
        if not completed_requests.empty:
            durations = completed_requests['metric_value']
            
            # Calculate statistics
            mean_duration = durations.mean()
            median_duration = durations.median()
            p95_duration = np.percentile(durations, 95)
            
            # Create histogram
            fig.add_trace(
                go.Histogram(
                    x=durations,
                    nbinsx=50,
                    name='Response Time Distribution',
                    marker_color='teal',
                    opacity=0.7,
                    hovertemplate='Response Time: %{x}ms<br>Count: %{y}<extra></extra>'
                ),
                row=2, col=1
            )
            
            # Add vertical lines for statistics
            fig.add_vline(x=mean_duration, line_dash="dash", line_color="red", line_width=2,
                          annotation_text=f"Mean: {mean_duration:.0f}ms", row=2, col=1)
            fig.add_vline(x=median_duration, line_dash="dash", line_color="green", line_width=2,
                          annotation_text=f"Median: {median_duration:.0f}ms", row=2, col=1)
            fig.add_vline(x=p95_duration, line_dash="dash", line_color="orange", line_width=2,
                          annotation_text=f"P95: {p95_duration:.0f}ms", row=2, col=1)
        else:
            fig.add_annotation(
                text="No completed request data",
                xref="x", yref="y",
                x=0.5, y=0.5,
                showarrow=False,
                row=2, col=1
            )
        
    except Exception as e:
        fig.add_annotation(
            text=f'Histogram error: {e}',
            xref="x", yref="y",
            x=0.5, y=0.5,
            showarrow=False,
            row=2, col=1
        )
    
    # Update layout
    fig.update_layout(
        height=800,
        title_text="Interactive Response Time Analysis",
        showlegend=False,
        font=dict(size=12)
    )
    
    # Update axes
    fig.update_xaxes(title_text="Percentile", row=1, col=1, title_font=dict(size=14))
    fig.update_yaxes(title_text="Response Time (ms)", row=1, col=1, title_font=dict(size=14))
    fig.update_xaxes(title_text="Response Time (ms)", row=2, col=1, title_font=dict(size=14))
    fig.update_yaxes(title_text="Number of Requests", row=2, col=1, title_font=dict(size=14))
    
    # Save the figure
    save_plotly_fig(fig, "comprehensive_response_plots.html", output_path)

def create_degradation_analysis(csv_data, output_path):
    """Creates analysis to find degradation point by response time and errors"""
    
    print("Creating degradation analysis...")
    
    # Process data with sampling
    duration_data = process_duration_data_chunked(csv_data, sample_fraction=0.05)
    
    if duration_data.empty:
        print("No response time data")
        return None
    
    # Create time intervals
    duration_data.loc[:, 'time_elapsed'] = (
        duration_data['timestamp_dt'] - duration_data['timestamp_dt'].min()
    ).dt.total_seconds()
    duration_data.loc[:, 'time_window'] = (duration_data['time_elapsed'] // 60).astype(int)
    
    # Analyze each interval
    window_stats = []
    unique_windows = sorted(duration_data['time_window'].unique())
    
    print(f"Processing {len(unique_windows)} time windows...")
    
    for i, window in enumerate(unique_windows):
        if i % 10 == 0 and i > 0:
            print(f"  Processed {i}/{len(unique_windows)} windows...")
            
        window_data = duration_data[duration_data['time_window'] == window]
        
        if len(window_data) < 5:
            continue
        
        # Response time statistics
        p50 = np.percentile(window_data['metric_value'], 50)
        p75 = np.percentile(window_data['metric_value'], 75) 
        p90 = np.percentile(window_data['metric_value'], 90)
        p95 = np.percentile(window_data['metric_value'], 95)
        p99 = np.percentile(window_data['metric_value'], 99)
        
        # Error statistics
        window_start = duration_data['timestamp_dt'].min() + pd.Timedelta(seconds=window*60)
        window_end = window_start + pd.Timedelta(seconds=60)
        
        error_count, error_rate = calculate_error_rate_chunked(csv_data, window, window_start, window_end)
        
        total_requests = len(window_data)
        
        window_stats.append({
            'time_window': window,
            'time_seconds': window * 60,
            'requests': total_requests,
            'p50': p50,
            'p75': p75, 
            'p90': p90,
            'p95': p95,
            'p99': p99,
            'error_count': error_count,
            'error_rate': error_rate,
            'degraded_time': p95 > 500,
            'degraded_errors': error_rate > 0.5
        })
    
    if not window_stats:
        print("Insufficient data for analysis")
        return None
    
    stats_df = pd.DataFrame(window_stats)
    
    # Create interactive degradation analysis with increased height
    fig = make_subplots(
        rows=3, cols=1,
        subplot_titles=['Response Time P95', 'Error Rate', 'Request Load'],
        vertical_spacing=0.1,
        row_heights=[0.4, 0.3, 0.3]
    )
    
    # Plot 1: P95 over time
    fig.add_trace(
        go.Scatter(
            x=stats_df['time_seconds'],
            y=stats_df['p95'],
            mode='lines+markers',
            name='P95 Response Time',
            line=dict(width=3, color='blue'),
            marker=dict(size=6),
            hovertemplate='Time: %{x}s<br>P95: %{y}ms<extra></extra>'
        ),
        row=1, col=1
    )
    
    # Degradation threshold line
    fig.add_hline(y=500, line_dash="dash", line_color="red", line_width=2,
                  annotation_text="Degradation Threshold (500ms)", 
                  annotation_font_size=12,
                  row=1, col=1)
    
    # Plot 2: Error rate over time
    fig.add_trace(
        go.Scatter(
            x=stats_df['time_seconds'],
            y=stats_df['error_rate'],
            mode='lines+markers',
            name='Error Rate',
            line=dict(width=3, color='red'),
            marker=dict(size=6),
            hovertemplate='Time: %{x}s<br>Error Rate: %{y}%<extra></extra>'
        ),
        row=2, col=1
    )
    
    # Error degradation threshold
    fig.add_hline(y=0.5, line_dash="dash", line_color="darkred", line_width=2,
                  annotation_text="Error Threshold (0.5%)", 
                  annotation_font_size=12,
                  row=2, col=1)
    
    # Plot 3: Request load over time
    fig.add_trace(
        go.Bar(
            x=stats_df['time_seconds'],
            y=stats_df['requests'],
            name='Requests per minute',
            marker_color='green',
            opacity=0.8,
            hovertemplate='Time: %{x}s<br>Requests: %{y}<extra></extra>'
        ),
        row=3, col=1
    )
    
    # Update layout with increased height and better styling
    fig.update_layout(
        height=1200,
        title_text="Performance Degradation Analysis",
        title_font_size=20,
        showlegend=True,
        font=dict(size=12)
    )
    
    # Update axes labels with larger fonts
    fig.update_xaxes(title_text="Time from start (seconds)", row=3, col=1, title_font=dict(size=14))
    fig.update_yaxes(title_text="P95 Response Time (ms)", row=1, col=1, title_font=dict(size=14))
    fig.update_yaxes(title_text="Error Rate (%)", row=2, col=1, title_font=dict(size=14))
    fig.update_yaxes(title_text="Number of Requests", row=3, col=1, title_font=dict(size=14))
    
    # Increase tick font size for all axes
    fig.update_xaxes(tickfont=dict(size=12), row=1, col=1)
    fig.update_yaxes(tickfont=dict(size=12), row=1, col=1)
    fig.update_xaxes(tickfont=dict(size=12), row=2, col=1)
    fig.update_yaxes(tickfont=dict(size=12), row=2, col=1)
    fig.update_xaxes(tickfont=dict(size=12), row=3, col=1)
    fig.update_yaxes(tickfont=dict(size=12), row=3, col=1)
    
    # Save the figure
    html_file = save_plotly_fig(fig, "degradation_analysis.html", output_path)
    
    # Print summary report
    print("\n" + "="*70)
    print("DEGRADATION POINT ANALYSIS SUMMARY")
    print("="*70)
    
    degraded_time = stats_df[stats_df['degraded_time']]
    degraded_errors = stats_df[stats_df['degraded_errors']]
    
    if not degraded_time.empty:
        first = degraded_time.iloc[0]
        print(f"RESPONSE TIME DEGRADATION:")
        print(f"   First occurrence: {first['time_seconds']} seconds")
        print(f"   Load: {first['requests']} requests/minute")
        print(f"   P95: {first['p95']:.0f} ms")
    
    if not degraded_errors.empty:
        first = degraded_errors.iloc[0]
        print(f"ERROR DEGRADATION:")
        print(f"   First occurrence: {first['time_seconds']} seconds")
        print(f"   Error rate: {first['error_rate']:.1f}%")
    
    print(f"\nOVERALL:")
    print(f"   Total requests analyzed: {len(duration_data):,}")
    print(f"   Time range: {stats_df['time_seconds'].min():.0f}-{stats_df['time_seconds'].max():.0f} seconds")
    print(f"   Max P95: {stats_df['p95'].max():.0f} ms")
    print(f"   Max error rate: {stats_df['error_rate'].max():.1f}%")
    
    return stats_df

def create_simplified_percentiles_chart(csv_data, output_path):
    """Creates simplified percentile chart for large datasets"""
    
    print("Creating simplified percentiles chart...")
    
    # Use sampling for large datasets
    duration_data = process_duration_data_chunked(csv_data, sample_fraction=0.02)
    
    if duration_data.empty:
        return
    
    # Use fewer time points for large datasets
    duration_data = duration_data.sort_values('timestamp_dt')
    duration_data['time_elapsed'] = (
        duration_data['timestamp_dt'] - duration_data['timestamp_dt'].min()
    ).dt.total_seconds()
    
    max_time = duration_data['time_elapsed'].max()
    n_points = min(50, int(max_time // 10) + 1)
    
    time_intervals = np.linspace(0, max_time, n_points)
    percentiles = [50, 75, 90, 95, 99]
    
    # Calculate percentiles
    percentile_data = {p: [] for p in percentiles}
    time_points = []
    
    for i in range(1, len(time_intervals)):
        interval_data = duration_data[
            (duration_data['time_elapsed'] >= time_intervals[i-1]) & 
            (duration_data['time_elapsed'] < time_intervals[i])
        ]['metric_value']
        
        if len(interval_data) > 5:
            time_points.append(time_intervals[i])
            for p in percentiles:
                percentile_data[p].append(np.percentile(interval_data, p))
    
    # Create plot
    fig = go.Figure()
    colors = ['green', 'blue', 'orange', 'red', 'purple']
    labels = ['P50', 'P75', 'P90', 'P95', 'P99']
    
    for p, color, label in zip(percentiles, colors, labels):
        if percentile_data[p]:
            fig.add_trace(go.Scatter(
                x=time_points,
                y=percentile_data[p],
                mode='lines',
                name=label,
                line=dict(width=2, color=color),
                hovertemplate=f'Time: %{{x}}s<br>{label}: %{{y}}ms<extra></extra>'
            ))
    
    fig.add_hline(y=500, line_dash="dash", line_color="black")
    
    fig.update_layout(
        title='Response Time Percentiles (Sampled Data)',
        xaxis_title='Test Time (seconds)',
        yaxis_title='Response Time (ms)',
        height=600
    )
    
    # Save the figure
    save_plotly_fig(fig, "percentiles_chart.html", output_path)

def create_basic_response_plots(csv_data, output_path):
    """Creates basic response time plots with sampling"""
    
    print("Creating basic response time plots...")
    
    # Sample data for large files
    duration_data = process_duration_data_chunked(csv_data, sample_fraction=0.05)
    
    if duration_data.empty:
        return
    
    # Basic percentiles
    percentiles = [50, 75, 90, 95, 99]
    p_values = [np.percentile(duration_data['metric_value'], p) for p in percentiles]
    
    fig1 = go.Figure()
    fig1.add_trace(go.Bar(
        x=[f'P{p}' for p in percentiles],
        y=p_values,
        marker_color=['lightgreen', 'lightblue', 'orange', 'red', 'darkred']
    ))
    
    fig1.update_layout(
        title='Response Time Percentiles',
        xaxis_title='Percentile',
        yaxis_title='Response Time (ms)',
        height=500
    )
    
    save_plotly_fig(fig1, "response_percentiles.html", output_path)
    
    # Basic histogram (sampled)
    fig2 = go.Figure()
    fig2.add_trace(go.Histogram(
        x=duration_data['metric_value'],
        nbinsx=50,
        marker_color='teal',
        opacity=0.7
    ))
    
    fig2.update_layout(
        title='Response Time Distribution',
        xaxis_title='Response Time (ms)',
        yaxis_title='Count',
        height=500
    )
    
    save_plotly_fig(fig2, "response_histogram.html", output_path)

def create_resource_plots(resource_data, output_path):
    """Creates resource usage plots if resource data is available"""
    
    if resource_data is None or resource_data.empty:
        print("No resource data available")
        return
    
    print("Creating resource usage plots...")
    
    containers = resource_data['container'].unique()
    
    for container in containers:
        if container not in ['src_app_1', 'src_db_1']:
            continue
            
        container_data = resource_data[resource_data['container'] == container]
        if container_data.empty:
            continue
            
        print(f"Creating charts for container: {container}")
        
        # Prepare data
        container_data = container_data.copy()
        
        # Convert timestamp to real time
        if 'timestamp' in container_data.columns:
            container_data['timestamp'] = pd.to_numeric(container_data['timestamp'], errors='coerce')
            container_data = container_data.dropna(subset=['timestamp'])
            start_time = container_data['timestamp'].min()
            real_time = (container_data['timestamp'] - start_time)
        else:
            real_time = range(len(container_data))
        
        # Create CPU usage chart
        try:
            container_data['cpu_numeric'] = container_data['cpu_percent'].str.replace('%', '').astype(float)
            
            fig = make_subplots(
                rows=2, cols=1,
                subplot_titles=[f'{container} - CPU Usage', f'{container} - Memory Usage'],
                vertical_spacing=0.12
            )
            
            # CPU usage subplot
            fig.add_trace(
                go.Scatter(
                    x=real_time,
                    y=container_data['cpu_numeric'],
                    mode='lines+markers',
                    name='CPU %',
                    line=dict(width=2, color='red'),
                    marker=dict(size=4),
                    hovertemplate='Time: %{x}s<br>CPU: %{y}%<extra></extra>'
                ),
                row=1, col=1
            )
            
            # Calculate and add statistics for CPU
            cpu_mean = container_data['cpu_numeric'].mean()
            cpu_max = container_data['cpu_numeric'].max()
            
            fig.add_hline(y=cpu_mean, line_dash="dash", line_color="blue", line_width=2,
                          annotation_text=f"Average CPU: {cpu_mean:.1f}%", row=1, col=1)
            fig.add_hline(y=cpu_max, line_dash="dash", line_color="orange", line_width=2,
                          annotation_text=f"Max CPU: {cpu_max:.1f}%", row=1, col=1)
            
            # Memory usage subplot
            def parse_memory(mem_str):
                try:
                    if pd.isna(mem_str):
                        return 0
                    used = mem_str.split('/')[0].strip()
                    used_num = float(''.join(filter(lambda x: x.isdigit() or x == '.', used)))
                    return used_num
                except:
                    return 0
            
            container_data['mem_used'] = container_data['mem_usage'].apply(parse_memory)
            
            fig.add_trace(
                go.Scatter(
                    x=real_time,
                    y=container_data['mem_used'],
                    mode='lines+markers',
                    name='Memory (MB)',
                    line=dict(width=2, color='green'),
                    marker=dict(size=4),
                    hovertemplate='Time: %{x}s<br>Memory: %{y} MB<extra></extra>'
                ),
                row=2, col=1
            )
            
            # Calculate and add statistics for memory
            mem_mean = container_data['mem_used'].mean()
            mem_max = container_data['mem_used'].max()
            
            fig.add_hline(y=mem_mean, line_dash="dash", line_color="blue", line_width=2,
                          annotation_text=f"Average Memory: {mem_mean:.1f} MB", row=2, col=1)
            fig.add_hline(y=mem_max, line_dash="dash", line_color="orange", line_width=2,
                          annotation_text=f"Max Memory: {mem_max:.1f} MB", row=2, col=1)
            
            fig.update_layout(
                height=800,
                title_text=f"Interactive Container Analysis: {container}",
                showlegend=False,
                font=dict(size=12)
            )
            
            fig.update_xaxes(title_text="Test Time (seconds)", row=1, col=1, title_font=dict(size=14))
            fig.update_yaxes(title_text="CPU %", row=1, col=1, title_font=dict(size=14))
            fig.update_xaxes(title_text="Test Time (seconds)", row=2, col=1, title_font=dict(size=14))
            fig.update_yaxes(title_text="Memory (MB)", row=2, col=1, title_font=dict(size=14))
            
            save_plotly_fig(fig, f"resources_{container}.html", output_path)
            
        except Exception as e:
            print(f"Error creating resource plot for {container}: {e}")

def comprehensive_analysis_large(csv_data, resource_data=None, output_path="."):
    """Performs comprehensive analysis optimized for large datasets"""
    
    print("=== COMPREHENSIVE ANALYSIS FOR LARGE DATASETS ===")
    
    # Create output directory if it doesn't exist
    os.makedirs(output_path, exist_ok=True)
    
    # Debug data structure
    debug_data_structure(csv_data)
    
    # Perform analyses with optimizations
    print("\n1. Creating degradation analysis chart...")
    stats_df = create_degradation_analysis(csv_data, output_path)
    
    print("\n2. Creating detailed percentiles chart...")
    create_detailed_percentiles_chart(csv_data, output_path)
    
    print("\n3. Creating cumulative percentiles chart...")
    create_cumulative_percentiles_chart(csv_data, output_path)
    
    print("\n4. Creating comprehensive response time plots...")
    create_comprehensive_response_plots(csv_data, output_path)
    
    print("\n5. Creating simplified percentiles chart...")
    create_simplified_percentiles_chart(csv_data, output_path)
    
    print("\n6. Creating basic response time plots...")
    create_basic_response_plots(csv_data, output_path)
    
    if resource_data is not None:
        print("\n7. Creating resource usage plots...")
        create_resource_plots(resource_data, output_path)
    
    print(f"\nANALYSIS COMPLETE!")
    print(f"All graphs have been saved as HTML files in: {output_path}")
    print(f"You can open them in your web browser.")
    
    return stats_df

if __name__ == "__main__":
    # Example usage for large files
    path = '/home/lus/7sem_testing_2ver/src/degradation_get_test_20251127_005636'
    output_dir = './degr_analysis_results'
    
    print(f"Processing large dataset from: {path}")
    print(f"Output directory: {output_dir}")
    
    try:
        # Load data with chunking and optimization
        csv_data = load_large_csv_in_chunks(path + '/k6_results.csv', chunk_size=50000)
        if csv_data.empty:
            print("Failed to load k6 results data")
            exit(1)
            
        csv_data = optimize_dataframe_memory(csv_data)
        
        # Load resource data if available
        resource_data = None
        resource_path = path + '/resource_usage.csv'
        if os.path.exists(resource_path):
            print("Loading resource data...")
            resource_data = load_large_csv_in_chunks(resource_path, chunk_size=50000)
            resource_data = optimize_dataframe_memory(resource_data)
        else:
            print("Resource data not found, skipping resource plots")
        
        # Perform analysis
        comprehensive_analysis_large(csv_data, resource_data, output_dir)
        
    except Exception as e:
        print(f"Error processing data: {e}")
        import traceback
        traceback.print_exc()



#------------------PNG------------------------------------------------------------------------
# import pandas as pd
# import numpy as np
# import plotly.graph_objects as go
# from plotly.subplots import make_subplots
# import plotly.express as px
# from datetime import datetime, timedelta
# import os
# from IPython.display import display, HTML
# import warnings
# warnings.filterwarnings('ignore')

# # Добавляем импорт для сохранения как PNG
# import plotly.io as pio

# def save_plotly_fig(fig, filename, path='.', format='png', width=2000, height=1200, scale=2):
#     """Save Plotly figure as PNG file with high quality"""
#     output_path = os.path.join(path, filename)
    
#     if format == 'png':
#         # Сохраняем как PNG с высоким качеством
#         pio.write_image(fig, output_path, format='png', width=width, height=height, scale=scale)
#         print(f"Graph saved as high-quality PNG: {output_path}")
#     elif format == 'html':
#         # Сохраняем как HTML (для интерактивности)
#         fig.write_html(output_path)
#         print(f"Graph saved as HTML: {output_path}")
#     else:
#         print(f"Unsupported format: {format}")
    
#     return output_path

# def load_large_csv_in_chunks(file_path, chunk_size=100000):
#     """Load large CSV file in chunks to avoid memory issues"""
#     print(f"Loading large CSV file in chunks: {file_path}")
#     chunks = []
#     total_rows = 0
    
#     try:
#         for chunk in pd.read_csv(file_path, chunksize=chunk_size):
#             chunks.append(chunk)
#             total_rows += len(chunk)
#             print(f"Loaded chunk {len(chunks)}: {len(chunk)} rows (total: {total_rows})")
        
#         if chunks:
#             return pd.concat(chunks, ignore_index=True)
#         else:
#             return pd.DataFrame()
#     except Exception as e:
#         print(f"Error loading CSV: {e}")
#         return pd.DataFrame()

# def optimize_dataframe_memory(df):
#     """Optimize dataframe memory usage"""
#     if df.empty:
#         return df
        
#     print("Optimizing dataframe memory usage...")
#     initial_memory = df.memory_usage(deep=True).sum() / 1024**2
    
#     # Convert numeric columns to appropriate types
#     numeric_columns = ['metric_value', 'timestamp']
#     for col in numeric_columns:
#         if col in df.columns:
#             df[col] = pd.to_numeric(df[col], errors='coerce', downcast='float')
    
#     # Convert categorical columns
#     categorical_columns = ['metric_name', 'status', 'error', 'container', 'cpu_percent', 'mem_usage']
#     for col in categorical_columns:
#         if col in df.columns and df[col].dtype == 'object':
#             df[col] = df[col].astype('category')
    
#     final_memory = df.memory_usage(deep=True).sum() / 1024**2
#     print(f"Memory optimization: {initial_memory:.2f} MB -> {final_memory:.2f} MB")
    
#     return df

# def debug_data_structure(csv_data):
#     """Function for debugging data structure"""
#     print("\nDEBUG DATA STRUCTURE:")
#     print(f"Total rows: {len(csv_data)}")
#     print(f"Columns: {list(csv_data.columns)}")
    
#     if not csv_data.empty:
#         print(f"Metric types: {csv_data['metric_name'].unique()}")
        
#         # Look at error data
#         error_data = csv_data[csv_data['metric_name'] == 'http_req_failed']
#         if not error_data.empty:
#             print(f"\nHTTP_REQ_FAILED data:")
#             print(f"   - Number of records: {len(error_data)}")
#             print(f"   - Values: {error_data['metric_value'].unique()[:10]}")
        
#         # Look at HTTP request statuses
#         http_requests = csv_data[csv_data['metric_name'] == 'http_req_duration']
#         if not http_requests.empty and 'status' in http_requests.columns:
#             print(f"\nHTTP statuses:")
#             print(f"   - Unique statuses: {http_requests['status'].unique()}")
#             print(f"   - Example statuses: {http_requests['status'].value_counts().head()}")
        
#         # Look at http_reqs data
#         http_reqs = csv_data[csv_data['metric_name'] == 'http_reqs']
#         if not http_reqs.empty:
#             print(f"\nHTTP_REQS data:")
#             print(f"   - Number of records: {len(http_reqs)}")
#             print(f"   - Values: {http_reqs['metric_value'].unique()[:10]}")
#             if 'timestamp' in http_reqs.columns:
#                 print(f"   - Timestamps: {http_reqs['timestamp'].head(3)}")
        
#         # Sample data for debugging
#         print(f"\nFirst 3 rows:")
#         print(csv_data.head(3))
        
#         # Memory usage info
#         print(f"\nMemory usage: {csv_data.memory_usage(deep=True).sum() / 1024**2:.2f} MB")

# def calculate_error_rate_chunked(csv_data, time_window, window_start, window_end):
#     """Calculate error rate using chunked processing for large datasets"""
#     try:
#         # Filter HTTP requests in this interval
#         http_requests = csv_data[
#             (csv_data['metric_name'] == 'http_req_duration')
#         ].copy()
        
#         if http_requests.empty:
#             return 0, 0
        
#         # Convert timestamp for filtering
#         http_requests.loc[:, 'timestamp_dt'] = pd.to_datetime(
#             http_requests['timestamp'], unit='s', errors='coerce'
#         )
#         http_requests = http_requests.dropna(subset=['timestamp_dt'])
        
#         # Filter by time window
#         window_requests = http_requests[
#             (http_requests['timestamp_dt'] >= window_start) & 
#             (http_requests['timestamp_dt'] < window_end)
#         ]
        
#         total_requests = len(window_requests)
        
#         if total_requests == 0:
#             return 0, 0
        
#         # Count errors by status (4xx, 5xx) or presence of error
#         error_count = 0
        
#         if 'status' in window_requests.columns:
#             # Process in chunks if too large
#             if len(window_requests) > 100000:
#                 chunk_size = 50000
#                 for i in range(0, len(window_requests), chunk_size):
#                     chunk = window_requests.iloc[i:i+chunk_size]
#                     status_series = pd.to_numeric(chunk['status'], errors='coerce')
#                     error_chunk = chunk[
#                         (status_series >= 400) |  # HTTP errors
#                         (chunk['error'].notna())  # Has error text
#                     ]
#                     error_count += len(error_chunk)
#             else:
#                 status_series = pd.to_numeric(window_requests['status'], errors='coerce')
#                 error_requests = window_requests[
#                     (status_series >= 400) |  # HTTP errors
#                     (window_requests['error'].notna())  # Has error text
#                 ]
#                 error_count = len(error_requests)
        
#         error_rate = (error_count / total_requests * 100) if total_requests > 0 else 0
        
#         return error_count, error_rate
        
#     except Exception as e:
#         print(f"Error calculating errors: {e}")
#         return 0, 0

# def process_duration_data_chunked(csv_data, sample_fraction=0.1):
#     """Process duration data with sampling for large datasets"""
#     print("Processing duration data with sampling...")
    
#     duration_data = csv_data[csv_data['metric_name'] == 'http_req_duration']
    
#     if duration_data.empty:
#         print("No response time data")
#         return pd.DataFrame()
    
#     print(f"Original duration data: {len(duration_data)} rows")
    
#     # Sample data if too large
#     if len(duration_data) > 100000:  # If more than 100K rows, sample
#         print(f"Large dataset detected. Sampling {sample_fraction*100}%...")
#         duration_data = duration_data.sample(frac=sample_fraction, random_state=42)
#         print(f"After sampling: {len(duration_data)} rows")
    
#     # Prepare data
#     duration_data = duration_data.copy()
#     duration_data['metric_value'] = pd.to_numeric(duration_data['metric_value'], errors='coerce')
#     duration_data = duration_data.dropna(subset=['metric_value'])
    
#     if 'timestamp' not in duration_data.columns:
#         print("No timestamps for analysis")
#         return pd.DataFrame()
    
#     # Convert timestamp
#     duration_data.loc[:, 'timestamp_dt'] = pd.to_datetime(
#         duration_data['timestamp'], unit='s', errors='coerce'
#     )
#     duration_data = duration_data.dropna(subset=['timestamp_dt'])
#     duration_data = duration_data.sort_values('timestamp_dt')
    
#     print(f"Final processed duration data: {len(duration_data)} rows")
#     return duration_data

# def create_detailed_percentiles_chart(csv_data, output_path):
#     """Creates detailed chart with 5 percentile functions over test time"""
    
#     print("Creating detailed percentiles chart...")
    
#     # Use sampling for large datasets
#     duration_data = process_duration_data_chunked(csv_data, sample_fraction=0.05)
    
#     if duration_data.empty:
#         return
    
#     # Prepare data
#     duration_data = duration_data.copy()
#     duration_data['timestamp'] = pd.to_numeric(duration_data['timestamp'], errors='coerce')
#     duration_data = duration_data.dropna(subset=['timestamp'])
    
#     if duration_data.empty:
#         return
    
#     # Sort by time and create time intervals
#     duration_data = duration_data.sort_values('timestamp')
#     duration_data['time_interval'] = (duration_data['timestamp'] - duration_data['timestamp'].min())
    
#     # Use all available time
#     max_time = duration_data['time_interval'].max()
    
#     # Break into time intervals
#     time_intervals = np.linspace(0, max_time, min(100, int(max_time) + 1))  # adaptive number of points
#     percentiles = [50, 75, 90, 95, 99]
#     colors = ['green', 'blue', 'orange', 'red', 'purple']
#     labels = ['P50', 'P75', 'P90', 'P95', 'P99']
    
#     # Calculate percentiles for each time interval
#     percentile_over_time = {p: [] for p in percentiles}
#     time_points = []
    
#     for i in range(1, len(time_intervals)):
#         time_start = time_intervals[i-1]
#         time_end = time_intervals[i]
        
#         # Data for current time interval
#         interval_data = duration_data[
#             (duration_data['time_interval'] >= time_start) & 
#             (duration_data['time_interval'] < time_end)
#         ]['metric_value']
        
#         if len(interval_data) > 0:
#             time_points.append(time_end)  # Use the end time point
#             for p in percentiles:
#                 p_value = np.percentile(interval_data, p)
#                 percentile_over_time[p].append(p_value)
#         else:
#             # Add NaN if no data in interval
#             time_points.append(time_end)
#             for p in percentiles:
#                 percentile_over_time[p].append(np.nan)
    
#     # Create interactive plot using Plotly
#     fig = go.Figure()
    
#     # Add each percentile as a line with markers
#     for p, color, label in zip(percentiles, colors, labels):
#         if len(percentile_over_time[p]) > 0:
#             fig.add_trace(go.Scatter(
#                 x=time_points,
#                 y=percentile_over_time[p],
#                 mode='lines+markers',
#                 name=label,
#                 line=dict(width=3),
#                 marker=dict(size=5),
#                 hovertemplate='Time: %{x}s<br>'+label+': %{y}ms<extra></extra>'
#             ))
    
#     # Add degradation threshold
#     fig.add_hline(y=500, line_dash="dash", line_color="black", line_width=2,
#                   annotation_text="Degradation Threshold (500 ms)", 
#                   annotation_font_size=12)
    
#     fig.update_layout(
#         title='Dynamics of Response Time Percentiles During Test',
#         xaxis_title='Test Time (seconds)',
#         yaxis_title='Response Time (milliseconds)',
#         height=700,
#         hovermode='x unified',
#         font=dict(size=12)
#     )
    
#     # Save the figure as PNG
#     save_plotly_fig(fig, "detailed_percentiles_chart.png", output_path, format='png', width=2000, height=1200)
    
#     print(f"Detailed percentiles chart saved")
#     print(f"Time range: 0-{max_time} seconds")

# def create_cumulative_percentiles_chart(csv_data, output_path):
#     """Creates interactive chart with cumulative percentiles (all data up to current moment)"""
    
#     print("Creating cumulative percentiles chart...")
    
#     # Use sampling for large datasets
#     duration_data = process_duration_data_chunked(csv_data, sample_fraction=0.05)
    
#     if duration_data.empty:
#         return
    
#     # Prepare data
#     duration_data = duration_data.copy()
#     duration_data['timestamp'] = pd.to_numeric(duration_data['timestamp'], errors='coerce')
#     duration_data = duration_data.dropna(subset=['timestamp'])
    
#     if duration_data.empty:
#         return
    
#     # Sort by time and create time intervals
#     duration_data = duration_data.sort_values('timestamp')
#     duration_data['time_interval'] = (duration_data['timestamp'] - duration_data['timestamp'].min())
    
#     # Use all available time
#     max_time = duration_data['time_interval'].max()
    
#     # Break into time intervals
#     time_intervals = np.linspace(0, max_time, min(100, int(max_time) + 1))
#     percentiles = [50, 75, 90, 95, 99]
#     colors = ['green', 'blue', 'orange', 'red', 'purple']
#     labels = ['P50', 'P75', 'P90', 'P95', 'P99']
    
#     # Calculate CUMULATIVE percentiles for each time interval
#     percentile_over_time = {p: [] for p in percentiles}
#     time_points = []
    
#     for i in range(1, len(time_intervals)):
#         time_end = time_intervals[i]
        
#         # Take ALL data up to this point in time
#         data_so_far = duration_data[duration_data['time_interval'] <= time_end]['metric_value']
        
#         if len(data_so_far) > 0:
#             time_points.append(time_end)
#             for p in percentiles:
#                 p_value = np.percentile(data_so_far, p)
#                 percentile_over_time[p].append(p_value)
#         else:
#             time_points.append(time_end)
#             for p in percentiles:
#                 percentile_over_time[p].append(np.nan)
    
#     # Create interactive plot
#     fig = go.Figure()
    
#     # Add each cumulative percentile as a line with markers
#     for p, color, label in zip(percentiles, colors, labels):
#         if len(percentile_over_time[p]) > 0:
#             fig.add_trace(go.Scatter(
#                 x=time_points,
#                 y=percentile_over_time[p],
#                 mode='lines+markers',
#                 name=label,
#                 line=dict(width=3),
#                 marker=dict(size=5),
#                 hovertemplate='Time: %{x}s<br>Cumulative '+label+': %{y}ms<extra></extra>'
#             ))
    
#     # Add degradation threshold
#     fig.add_hline(y=500, line_dash="dash", line_color="black", line_width=2,
#                   annotation_text="Degradation Threshold (500 ms)", 
#                   annotation_font_size=12)
    
#     fig.update_layout(
#         title='Cumulative Response Time Percentiles (All Data Up to Current Moment)',
#         xaxis_title='Test Time (seconds)',
#         yaxis_title='Response Time (milliseconds)',
#         height=700,
#         hovermode='x unified',
#         font=dict(size=12)
#     )
    
#     # Save the figure as PNG
#     save_plotly_fig(fig, "cumulative_percentiles_chart.png", output_path, format='png', width=2000, height=1200)
    
#     print(f"Cumulative percentiles chart saved")
#     print(f"Time range: 0-{max_time} seconds")

# def create_comprehensive_response_plots(csv_data, output_path):
#     """Creates comprehensive response time plots with all percentiles"""
    
#     print("Creating comprehensive response time plots...")
    
#     # Sample data for large files
#     duration_data = process_duration_data_chunked(csv_data, sample_fraction=0.05)
    
#     if duration_data.empty:
#         return
    
#     # Create subplot with 2 charts
#     fig = make_subplots(
#         rows=2, cols=1,
#         subplot_titles=['Response Time by Percentiles (Completed Requests)', 'Response Time Distribution (Completed Requests)'],
#         vertical_spacing=0.12
#     )
    
#     # Plot 1: Response time by percentiles
#     try:
#         completed_requests = csv_data[csv_data['metric_name'] == 'http_req_duration']
        
#         if not completed_requests.empty:
#             percentiles = [50, 75, 90, 95, 99]
#             percentile_values = [np.percentile(completed_requests['metric_value'], p) for p in percentiles]
            
#             fig.add_trace(
#                 go.Bar(
#                     x=[f'P{p}' for p in percentiles],
#                     y=percentile_values,
#                     name='Response Time by Percentiles',
#                     marker_color=['lightgreen', 'lightblue', 'orange', 'red', 'darkred'],
#                     hovertemplate='Percentile: %{x}<br>Response Time: %{y}ms<extra></extra>'
#                 ),
#                 row=1, col=1
#             )
#         else:
#             fig.add_annotation(
#                 text="No completed request data",
#                 xref="x", yref="y",
#                 x=0.5, y=0.5,
#                 showarrow=False,
#                 row=1, col=1
#             )
        
#     except Exception as e:
#         fig.add_annotation(
#             text=f'Percentile error: {e}',
#             xref="x", yref="y",
#             x=0.5, y=0.5,
#             showarrow=False,
#             row=1, col=1
#         )
    
#     # Plot 2: Response time histogram
#     try:
#         completed_requests = csv_data[csv_data['metric_name'] == 'http_req_duration']
        
#         if not completed_requests.empty:
#             durations = completed_requests['metric_value']
            
#             # Calculate statistics
#             mean_duration = durations.mean()
#             median_duration = durations.median()
#             p95_duration = np.percentile(durations, 95)
            
#             # Create histogram
#             fig.add_trace(
#                 go.Histogram(
#                     x=durations,
#                     nbinsx=50,
#                     name='Response Time Distribution',
#                     marker_color='teal',
#                     opacity=0.7,
#                     hovertemplate='Response Time: %{x}ms<br>Count: %{y}<extra></extra>'
#                 ),
#                 row=2, col=1
#             )
            
#             # Add vertical lines for statistics
#             fig.add_vline(x=mean_duration, line_dash="dash", line_color="red", line_width=2,
#                           annotation_text=f"Mean: {mean_duration:.0f}ms", row=2, col=1)
#             fig.add_vline(x=median_duration, line_dash="dash", line_color="green", line_width=2,
#                           annotation_text=f"Median: {median_duration:.0f}ms", row=2, col=1)
#             fig.add_vline(x=p95_duration, line_dash="dash", line_color="orange", line_width=2,
#                           annotation_text=f"P95: {p95_duration:.0f}ms", row=2, col=1)
#         else:
#             fig.add_annotation(
#                 text="No completed request data",
#                 xref="x", yref="y",
#                 x=0.5, y=0.5,
#                 showarrow=False,
#                 row=2, col=1
#             )
        
#     except Exception as e:
#         fig.add_annotation(
#             text=f'Histogram error: {e}',
#             xref="x", yref="y",
#             x=0.5, y=0.5,
#             showarrow=False,
#             row=2, col=1
#         )
    
#     # Update layout
#     fig.update_layout(
#         height=800,
#         title_text="Interactive Response Time Analysis",
#         showlegend=False,
#         font=dict(size=12)
#     )
    
#     # Update axes
#     fig.update_xaxes(title_text="Percentile", row=1, col=1, title_font=dict(size=14))
#     fig.update_yaxes(title_text="Response Time (ms)", row=1, col=1, title_font=dict(size=14))
#     fig.update_xaxes(title_text="Response Time (ms)", row=2, col=1, title_font=dict(size=14))
#     fig.update_yaxes(title_text="Number of Requests", row=2, col=1, title_font=dict(size=14))
    
#     # Save the figure as PNG
#     save_plotly_fig(fig, "comprehensive_response_plots.png", output_path, format='png', width=2000, height=1600)

# def create_degradation_analysis(csv_data, output_path):
#     """Creates analysis to find degradation point by response time and errors"""
    
#     print("Creating degradation analysis...")
    
#     # Process data with sampling
#     duration_data = process_duration_data_chunked(csv_data, sample_fraction=0.05)
    
#     if duration_data.empty:
#         print("No response time data")
#         return None
    
#     # Create time intervals
#     duration_data.loc[:, 'time_elapsed'] = (
#         duration_data['timestamp_dt'] - duration_data['timestamp_dt'].min()
#     ).dt.total_seconds()
#     duration_data.loc[:, 'time_window'] = (duration_data['time_elapsed'] // 60).astype(int)
    
#     # Analyze each interval
#     window_stats = []
#     unique_windows = sorted(duration_data['time_window'].unique())
    
#     print(f"Processing {len(unique_windows)} time windows...")
    
#     for i, window in enumerate(unique_windows):
#         if i % 10 == 0 and i > 0:
#             print(f"  Processed {i}/{len(unique_windows)} windows...")
            
#         window_data = duration_data[duration_data['time_window'] == window]
        
#         if len(window_data) < 5:
#             continue
        
#         # Response time statistics
#         p50 = np.percentile(window_data['metric_value'], 50)
#         p75 = np.percentile(window_data['metric_value'], 75) 
#         p90 = np.percentile(window_data['metric_value'], 90)
#         p95 = np.percentile(window_data['metric_value'], 95)
#         p99 = np.percentile(window_data['metric_value'], 99)
        
#         # Error statistics
#         window_start = duration_data['timestamp_dt'].min() + pd.Timedelta(seconds=window*60)
#         window_end = window_start + pd.Timedelta(seconds=60)
        
#         error_count, error_rate = calculate_error_rate_chunked(csv_data, window, window_start, window_end)
        
#         total_requests = len(window_data)
        
#         window_stats.append({
#             'time_window': window,
#             'time_seconds': window * 60,
#             'requests': total_requests,
#             'p50': p50,
#             'p75': p75, 
#             'p90': p90,
#             'p95': p95,
#             'p99': p99,
#             'error_count': error_count,
#             'error_rate': error_rate,
#             'degraded_time': p95 > 500,
#             'degraded_errors': error_rate > 0.5
#         })
    
#     if not window_stats:
#         print("Insufficient data for analysis")
#         return None
    
#     stats_df = pd.DataFrame(window_stats)
    
#     # Create interactive degradation analysis with increased height
#     fig = make_subplots(
#         rows=3, cols=1,
#         subplot_titles=['Response Time P95', 'Error Rate', 'Request Load'],
#         vertical_spacing=0.1,
#         row_heights=[0.4, 0.3, 0.3]
#     )
    
#     # Plot 1: P95 over time
#     fig.add_trace(
#         go.Scatter(
#             x=stats_df['time_seconds'],
#             y=stats_df['p95'],
#             mode='lines+markers',
#             name='P95 Response Time',
#             line=dict(width=3, color='blue'),
#             marker=dict(size=6),
#             hovertemplate='Time: %{x}s<br>P95: %{y}ms<extra></extra>'
#         ),
#         row=1, col=1
#     )
    
#     # Degradation threshold line
#     fig.add_hline(y=500, line_dash="dash", line_color="red", line_width=2,
#                   annotation_text="Degradation Threshold (500ms)", 
#                   annotation_font_size=12,
#                   row=1, col=1)
    
#     # Plot 2: Error rate over time
#     fig.add_trace(
#         go.Scatter(
#             x=stats_df['time_seconds'],
#             y=stats_df['error_rate'],
#             mode='lines+markers',
#             name='Error Rate',
#             line=dict(width=3, color='red'),
#             marker=dict(size=6),
#             hovertemplate='Time: %{x}s<br>Error Rate: %{y}%<extra></extra>'
#         ),
#         row=2, col=1
#     )
    
#     # Error degradation threshold
#     fig.add_hline(y=0.5, line_dash="dash", line_color="darkred", line_width=2,
#                   annotation_text="Error Threshold (0.5%)", 
#                   annotation_font_size=12,
#                   row=2, col=1)
    
#     # Plot 3: Request load over time
#     fig.add_trace(
#         go.Bar(
#             x=stats_df['time_seconds'],
#             y=stats_df['requests'],
#             name='Requests per minute',
#             marker_color='green',
#             opacity=0.8,
#             hovertemplate='Time: %{x}s<br>Requests: %{y}<extra></extra>'
#         ),
#         row=3, col=1
#     )
    
#     # Update layout with increased height and better styling
#     fig.update_layout(
#         height=1200,
#         title_text="Performance Degradation Analysis",
#         title_font_size=20,
#         showlegend=True,
#         font=dict(size=12)
#     )
    
#     # Update axes labels with larger fonts
#     fig.update_xaxes(title_text="Time from start (seconds)", row=3, col=1, title_font=dict(size=14))
#     fig.update_yaxes(title_text="P95 Response Time (ms)", row=1, col=1, title_font=dict(size=14))
#     fig.update_yaxes(title_text="Error Rate (%)", row=2, col=1, title_font=dict(size=14))
#     fig.update_yaxes(title_text="Number of Requests", row=3, col=1, title_font=dict(size=14))
    
#     # Increase tick font size for all axes
#     fig.update_xaxes(tickfont=dict(size=12), row=1, col=1)
#     fig.update_yaxes(tickfont=dict(size=12), row=1, col=1)
#     fig.update_xaxes(tickfont=dict(size=12), row=2, col=1)
#     fig.update_yaxes(tickfont=dict(size=12), row=2, col=1)
#     fig.update_xaxes(tickfont=dict(size=12), row=3, col=1)
#     fig.update_yaxes(tickfont=dict(size=12), row=3, col=1)
    
#     # Save the figure as PNG
#     save_plotly_fig(fig, "degradation_analysis.png", output_path, format='png', width=2000, height=1600)
    
#     # Print summary report
#     print("\n" + "="*70)
#     print("DEGRADATION POINT ANALYSIS SUMMARY")
#     print("="*70)
    
#     degraded_time = stats_df[stats_df['degraded_time']]
#     degraded_errors = stats_df[stats_df['degraded_errors']]
    
#     if not degraded_time.empty:
#         first = degraded_time.iloc[0]
#         print(f"RESPONSE TIME DEGRADATION:")
#         print(f"   First occurrence: {first['time_seconds']} seconds")
#         print(f"   Load: {first['requests']} requests/minute")
#         print(f"   P95: {first['p95']:.0f} ms")
    
#     if not degraded_errors.empty:
#         first = degraded_errors.iloc[0]
#         print(f"ERROR DEGRADATION:")
#         print(f"   First occurrence: {first['time_seconds']} seconds")
#         print(f"   Error rate: {first['error_rate']:.1f}%")
    
#     print(f"\nOVERALL:")
#     print(f"   Total requests analyzed: {len(duration_data):,}")
#     print(f"   Time range: {stats_df['time_seconds'].min():.0f}-{stats_df['time_seconds'].max():.0f} seconds")
#     print(f"   Max P95: {stats_df['p95'].max():.0f} ms")
#     print(f"   Max error rate: {stats_df['error_rate'].max():.1f}%")
    
#     return stats_df

# def create_simplified_percentiles_chart(csv_data, output_path):
#     """Creates simplified percentile chart for large datasets"""
    
#     print("Creating simplified percentiles chart...")
    
#     # Use sampling for large datasets
#     duration_data = process_duration_data_chunked(csv_data, sample_fraction=0.02)
    
#     if duration_data.empty:
#         return
    
#     # Use fewer time points for large datasets
#     duration_data = duration_data.sort_values('timestamp_dt')
#     duration_data['time_elapsed'] = (
#         duration_data['timestamp_dt'] - duration_data['timestamp_dt'].min()
#     ).dt.total_seconds()
    
#     max_time = duration_data['time_elapsed'].max()
#     n_points = min(50, int(max_time // 10) + 1)
    
#     time_intervals = np.linspace(0, max_time, n_points)
#     percentiles = [50, 75, 90, 95, 99]
    
#     # Calculate percentiles
#     percentile_data = {p: [] for p in percentiles}
#     time_points = []
    
#     for i in range(1, len(time_intervals)):
#         interval_data = duration_data[
#             (duration_data['time_elapsed'] >= time_intervals[i-1]) & 
#             (duration_data['time_elapsed'] < time_intervals[i])
#         ]['metric_value']
        
#         if len(interval_data) > 5:
#             time_points.append(time_intervals[i])
#             for p in percentiles:
#                 percentile_data[p].append(np.percentile(interval_data, p))
    
#     # Create plot
#     fig = go.Figure()
#     colors = ['green', 'blue', 'orange', 'red', 'purple']
#     labels = ['P50', 'P75', 'P90', 'P95', 'P99']
    
#     for p, color, label in zip(percentiles, colors, labels):
#         if percentile_data[p]:
#             fig.add_trace(go.Scatter(
#                 x=time_points,
#                 y=percentile_data[p],
#                 mode='lines',
#                 name=label,
#                 line=dict(width=2, color=color),
#                 hovertemplate=f'Time: %{{x}}s<br>{label}: %{{y}}ms<extra></extra>'
#             ))
    
#     fig.add_hline(y=500, line_dash="dash", line_color="black")
    
#     fig.update_layout(
#         title='Response Time Percentiles (Sampled Data)',
#         xaxis_title='Test Time (seconds)',
#         yaxis_title='Response Time (ms)',
#         height=600
#     )
    
#     # Save the figure as PNG
#     save_plotly_fig(fig, "percentiles_chart.png", output_path, format='png', width=2000, height=1200)

# def create_basic_response_plots(csv_data, output_path):
#     """Creates basic response time plots with sampling"""
    
#     print("Creating basic response time plots...")
    
#     # Sample data for large files
#     duration_data = process_duration_data_chunked(csv_data, sample_fraction=0.05)
    
#     if duration_data.empty:
#         return
    
#     # Basic percentiles
#     percentiles = [50, 75, 90, 95, 99]
#     p_values = [np.percentile(duration_data['metric_value'], p) for p in percentiles]
    
#     fig1 = go.Figure()
#     fig1.add_trace(go.Bar(
#         x=[f'P{p}' for p in percentiles],
#         y=p_values,
#         marker_color=['lightgreen', 'lightblue', 'orange', 'red', 'darkred']
#     ))
    
#     fig1.update_layout(
#         title='Response Time Percentiles',
#         xaxis_title='Percentile',
#         yaxis_title='Response Time (ms)',
#         height=500
#     )
    
#     save_plotly_fig(fig1, "response_percentiles.png", output_path, format='png', width=1600, height=1000)
    
#     # Basic histogram (sampled)
#     fig2 = go.Figure()
#     fig2.add_trace(go.Histogram(
#         x=duration_data['metric_value'],
#         nbinsx=50,
#         marker_color='teal',
#         opacity=0.7
#     ))
    
#     fig2.update_layout(
#         title='Response Time Distribution',
#         xaxis_title='Response Time (ms)',
#         yaxis_title='Count',
#         height=500
#     )
    
#     save_plotly_fig(fig2, "response_histogram.png", output_path, format='png', width=1600, height=1000)

# def create_resource_plots(resource_data, output_path):
#     """Creates resource usage plots if resource data is available"""
    
#     if resource_data is None or resource_data.empty:
#         print("No resource data available")
#         return
    
#     print("Creating resource usage plots...")
    
#     containers = resource_data['container'].unique()
    
#     for container in containers:
#         if container not in ['src_app_1', 'src_db_1']:
#             continue
            
#         container_data = resource_data[resource_data['container'] == container]
#         if container_data.empty:
#             continue
            
#         print(f"Creating charts for container: {container}")
        
#         # Prepare data
#         container_data = container_data.copy()
        
#         # Convert timestamp to real time
#         if 'timestamp' in container_data.columns:
#             container_data['timestamp'] = pd.to_numeric(container_data['timestamp'], errors='coerce')
#             container_data = container_data.dropna(subset=['timestamp'])
#             start_time = container_data['timestamp'].min()
#             real_time = (container_data['timestamp'] - start_time)
#         else:
#             real_time = range(len(container_data))
        
#         # Create CPU usage chart
#         try:
#             container_data['cpu_numeric'] = container_data['cpu_percent'].str.replace('%', '').astype(float)
            
#             fig = make_subplots(
#                 rows=2, cols=1,
#                 subplot_titles=[f'{container} - CPU Usage', f'{container} - Memory Usage'],
#                 vertical_spacing=0.12
#             )
            
#             # CPU usage subplot
#             fig.add_trace(
#                 go.Scatter(
#                     x=real_time,
#                     y=container_data['cpu_numeric'],
#                     mode='lines+markers',
#                     name='CPU %',
#                     line=dict(width=2, color='red'),
#                     marker=dict(size=4),
#                     hovertemplate='Time: %{x}s<br>CPU: %{y}%<extra></extra>'
#                 ),
#                 row=1, col=1
#             )
            
#             # Calculate and add statistics for CPU
#             cpu_mean = container_data['cpu_numeric'].mean()
#             cpu_max = container_data['cpu_numeric'].max()
            
#             fig.add_hline(y=cpu_mean, line_dash="dash", line_color="blue", line_width=2,
#                           annotation_text=f"Average CPU: {cpu_mean:.1f}%", row=1, col=1)
#             fig.add_hline(y=cpu_max, line_dash="dash", line_color="orange", line_width=2,
#                           annotation_text=f"Max CPU: {cpu_max:.1f}%", row=1, col=1)
            
#             # Memory usage subplot
#             def parse_memory(mem_str):
#                 try:
#                     if pd.isna(mem_str):
#                         return 0
#                     used = mem_str.split('/')[0].strip()
#                     used_num = float(''.join(filter(lambda x: x.isdigit() or x == '.', used)))
#                     return used_num
#                 except:
#                     return 0
            
#             container_data['mem_used'] = container_data['mem_usage'].apply(parse_memory)
            
#             fig.add_trace(
#                 go.Scatter(
#                     x=real_time,
#                     y=container_data['mem_used'],
#                     mode='lines+markers',
#                     name='Memory (MB)',
#                     line=dict(width=2, color='green'),
#                     marker=dict(size=4),
#                     hovertemplate='Time: %{x}s<br>Memory: %{y} MB<extra></extra>'
#                 ),
#                 row=2, col=1
#             )
            
#             # Calculate and add statistics for memory
#             mem_mean = container_data['mem_used'].mean()
#             mem_max = container_data['mem_used'].max()
            
#             fig.add_hline(y=mem_mean, line_dash="dash", line_color="blue", line_width=2,
#                           annotation_text=f"Average Memory: {mem_mean:.1f} MB", row=2, col=1)
#             fig.add_hline(y=mem_max, line_dash="dash", line_color="orange", line_width=2,
#                           annotation_text=f"Max Memory: {mem_max:.1f} MB", row=2, col=1)
            
#             fig.update_layout(
#                 height=800,
#                 title_text=f"Interactive Container Analysis: {container}",
#                 showlegend=False,
#                 font=dict(size=12)
#             )
            
#             fig.update_xaxes(title_text="Test Time (seconds)", row=1, col=1, title_font=dict(size=14))
#             fig.update_yaxes(title_text="CPU %", row=1, col=1, title_font=dict(size=14))
#             fig.update_xaxes(title_text="Test Time (seconds)", row=2, col=1, title_font=dict(size=14))
#             fig.update_yaxes(title_text="Memory (MB)", row=2, col=1, title_font=dict(size=14))
            
#             save_plotly_fig(fig, f"resources_{container}.png", output_path, format='png', width=2000, height=1600)
            
#         except Exception as e:
#             print(f"Error creating resource plot for {container}: {e}")

# def comprehensive_analysis_large(csv_data, resource_data=None, output_path="."):
#     """Performs comprehensive analysis optimized for large datasets"""
    
#     print("=== COMPREHENSIVE ANALYSIS FOR LARGE DATASETS ===")
    
#     # Create output directory if it doesn't exist
#     os.makedirs(output_path, exist_ok=True)
    
#     # Debug data structure
#     debug_data_structure(csv_data)
    
#     # Perform analyses with optimizations
#     print("\n1. Creating degradation analysis chart...")
#     stats_df = create_degradation_analysis(csv_data, output_path)
    
#     print("\n2. Creating detailed percentiles chart...")
#     create_detailed_percentiles_chart(csv_data, output_path)
    
#     print("\n3. Creating cumulative percentiles chart...")
#     create_cumulative_percentiles_chart(csv_data, output_path)
    
#     print("\n4. Creating comprehensive response time plots...")
#     create_comprehensive_response_plots(csv_data, output_path)
    
#     print("\n5. Creating simplified percentiles chart...")
#     create_simplified_percentiles_chart(csv_data, output_path)
    
#     print("\n6. Creating basic response time plots...")
#     create_basic_response_plots(csv_data, output_path)
    
#     if resource_data is not None:
#         print("\n7. Creating resource usage plots...")
#         create_resource_plots(resource_data, output_path)
    
#     print(f"\nANALYSIS COMPLETE!")
#     print(f"All graphs have been saved as PNG files in: {output_path}")
#     print(f"Graphs are saved with high resolution (2000x1200px and larger)")
    
#     return stats_df

# if __name__ == "__main__":
#     # Example usage for large files
#     path = '/home/lus/7sem_testing_2ver/src/degradation_get_test_20251127_005636'
#     output_dir = './analysis_results'
    
#     print(f"Processing large dataset from: {path}")
#     print(f"Output directory: {output_dir}")
    
#     try:
#         # Load data with chunking and optimization
#         csv_data = load_large_csv_in_chunks(path + '/k6_results.csv', chunk_size=50000)
#         if csv_data.empty:
#             print("Failed to load k6 results data")
#             exit(1)
            
#         csv_data = optimize_dataframe_memory(csv_data)
        
#         # Load resource data if available
#         resource_data = None
#         resource_path = path + '/resource_usage.csv'
#         if os.path.exists(resource_path):
#             print("Loading resource data...")
#             resource_data = load_large_csv_in_chunks(resource_path, chunk_size=50000)
#             resource_data = optimize_dataframe_memory(resource_data)
#         else:
#             print("Resource data not found, skipping resource plots")
        
#         # Perform analysis
#         comprehensive_analysis_large(csv_data, resource_data, output_dir)
        
#     except Exception as e:
#         print(f"Error processing data: {e}")
#         import traceback
#         traceback.print_exc()